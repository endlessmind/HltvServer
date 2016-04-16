using HltvRss.Classes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using Supremes;
using Supremes.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace HltvRss.Scorebot
{
    class ScorebotService
    {
        public delegate void UpdatetHandler(object sender, ScorebotUpdateEventArgs e);
        public event UpdatetHandler OnStatusUpdateConnected;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private Socket socket;
        bool connected = false;
        private int port = 10022;
        private String url;
        List<ScorebotItem> sbItems;
        IO.Options opt;
        List<Match> matches;

        public ScorebotService(String u)
        {
            url = u;
            sbItems = new List<ScorebotItem>();
            var options = new IO.Options();
            options.Timeout = 5000 * 2;
            opt = options;
        }


        private void MatchTeamWithID(int id, Match m)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.hltv.org/?pageid=179&teamid=" + id + "&gameid=2");
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            } catch (Exception e1)
            {
                return; //No response from server for some reason. Let's just move on for now
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();


                Document doc = Dcsoup.Parse(data);
                Elements el = doc.Select("div[class=covSmallHeadline][style=width:100%;float:left;]");

                String name = el.First().Text; //This should be the name

                //Matching id with team-name
               // foreach (var m in matches)
               // {
                    int index = matches.IndexOf(m);
                    Team t1 = m.TeamOne;
                    Team t2 = m.TeamTwo;

                    if (t1.Name.ToLower() == name.ToLower())
                    {
                        t1.ID = id;
                        m.TeamOne = t1;
                    }

                    if (t2.Name.ToLower() == name.ToLower())
                    {
                        t2.ID = id;
                        m.TeamTwo = t2;
                    }

                    //matches[index] = m;

              //  }


            }
        }

        private void CheckID()
        {
            Thread.Sleep(1200);
            foreach (var m in matches)
            {
                
                Thread.Sleep(50);
                allDone.Reset();
                socket.Emit("readyForMatch", m.ID);
                allDone.WaitOne();
            }

            for (int i = 0; i < sbItems.Count(); i++)
            {
                ScorebotItem itm = sbItems[i];
                var mas = from ma in matches where ma.ID == itm.ID select ma;
                if (mas != null && mas.Any())
                {
                    var ma = mas.First();
                    Match OldM = (Match)ma;
                    int index = matches.IndexOf(OldM);
                    OldM.hasScorebot = itm.isLive;

                    if (itm.isLive && itm.TeamIDs != null)
                    {
                        foreach (int id in itm.TeamIDs)
                        {
                            MatchTeamWithID(id, OldM);
                        }
                    }
                    
                    matches[index] = OldM;

                }
            }
            if (OnStatusUpdateConnected != null)
            {
                ScorebotUpdateEventArgs args = new ScorebotUpdateEventArgs(matches);
                OnStatusUpdateConnected(this, args);
            }
        }

        public void Connect(List<Match> m)
        {
            matches = m;
            sbItems.Clear();
            
            if (!connected){
                Console.WriteLine("Scorebot: " + url + ":10022/");
                socket = IO.Socket(url + ":10022/");

                socket.On(Socket.EVENT_CONNECT, () =>
                {

                    connected = true;

                    socket.On("scoreboard", (data) =>
                    {

                    });

                        socket.On("score", (data) =>
                    {

                        ScorebotItem itm = new ScorebotItem();
                        var stuff = JObject.Parse(data.ToString());

                        int MatchID = int.Parse(stuff.SelectToken("listId").ToString());
                        bool isLive = Boolean.Parse(stuff.SelectToken("matchLive").ToString());

                        var current = (JObject)stuff.GetValue("currentMap");
                        int currentCT = int.Parse(current.GetValue("currentCTTeam") + "");
                        int currentT = int.Parse(current.GetValue("currentTTeam") + "");

                        //If current-ids is equal to -1
                        if (currentCT == -1 && currentT == -1)
                        {
                            if (stuff.GetValue("mapScores").HasValues)
                            {

                                var score = (JObject)stuff.GetValue("mapScores");
                                if (score.HasValues)
                                {
                                    var jData = (JObject)((JObject)score.GetValue("1")).GetValue("firstHalf");
                                    List<int> ids = new List<int>();
                                    ids.Add(int.Parse(jData.GetValue("ctTeamDbId") + ""));
                                    ids.Add(int.Parse(jData.GetValue("tTeamDbId") + ""));

                                    itm.TeamIDs = ids;


                                }

                            }
                            else
                            {
                            }
                        } else
                        {
                            List<int> ids = new List<int>();
                            ids.Add(currentCT);
                            ids.Add(currentT);

                            itm.TeamIDs = ids;
                        }

                        itm.ID = MatchID;
                        itm.isLive = isLive;
                        sbItems.Add(itm);

                        allDone.Set();
                    });

                    Console.WriteLine("Connected!");
                    new Thread(CheckID).Start();

                });

                socket.On(Socket.EVENT_ERROR, (data) =>
                {
                    connected = false;
                });


                socket.Connect();

            } else
            {
                new Thread(CheckID).Start();
            }
        }

    }
}
