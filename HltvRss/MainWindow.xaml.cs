using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.IO;
using Supremes.Nodes;
using HltvRss.Utils;
using HltvRss.Classes;
using ExCSS;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using HltvRss.Server;
using HltvRss.RSS;
using System.Web.Script.Serialization;
using HltvRss.GCM.Class;
using HltvRss.GCM;
using OpenHardwareMonitor.Hardware;
using HltvRss.Server.Classes;
using HltvRss.Scorebot;

namespace HltvRss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int maxDataPoints = 288; //With 10min between each point will give us 48h of data
        const int maxRecentUpdate = 8;
        int currentRecentUpdate = 0;

        private DateTime startTime = DateTime.Now;
        private String updateString = "";

        DispatcherTimer latestUpdateTimer = new DispatcherTimer();
        DispatcherTimer RunningTimer = new DispatcherTimer();
        DispatcherTimer ChartTimer = new DispatcherTimer();
        DispatcherTimer ScorebotTimer = new DispatcherTimer();

        List<Match> hotMatches = new List<Match>();
        List<Match> latestResult = new List<Match>();
        List<String> RemovedUrls = new List<String>();
        List<String> AddedUrls = new List<String>();

        //TODO: Custom object class that also holds a date-string inc time
        List<CharItemLong> clientsChartData = new List<CharItemLong>();
        List<CharItemLong> SockSentChartData = new List<CharItemLong>();
        List<long> MemoryChartData = new List<long>();
        List<long> SockReceivedChartData = new List<long>();
        List<float> TempChartData = new List<float>();

        List<long> memoryCollection = new List<long>();

        float cpuTemp;

        TCPServer server;
        //RSS_Service RSS;
        TempMonitor temp;
        ScorebotService scorebot;


        String cpuTemperature;
        String moboTemperature;

        long lastSocketReceived;
        long lastSocketSent;
        long lastHtml;
        long lastClient;


        long gcmSent;
        long dataSize;
        long socketDataSent = 0;
        long socketDataReceived = 0;
        long totalClientConnected = 0;
        long LastUpdate = 0;
        long AllowedLastUpdate = 1000 * 90;

        private long mAdded = 0;
        private long mRemoved = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        Stopwatch stopwatch;

        private void ClientConnected(object sender, ClientConnectEventArgs e)
        {
            e.ConnectedClient.OnClientReceive += new Client.NewClientHandler(ClientReceived);

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
           (ThreadStart)delegate ()
           {
               lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
               lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
               lbsock.Content = "Sock: " + StringUtils.FormateFileSize(socketDataSent);
               lbClient.Content = "Total client: " + totalClientConnected;
               lbGcm.Content = "Total gcm: " + this.gcmSent;
           });

        }


        private void ClientReceived(object sender, ClientMsgReceivedEventArgs e)
        {
            String msg = e.State.sb.ToString().Replace("<EOF>", "").Replace("\n", "");
            
            Client c = e.ConnectedClient;
            JavaScriptSerializer serial = new JavaScriptSerializer();
            

            //Don't count this data or client connections
            if (msg.ToLower() == App.CMD_REQUEST_SERVER_STATUS)
            {
                StatusCollectionItem obj = new StatusCollectionItem();
                obj.HotMatchesCount = hotMatches.Count;
                obj.HtmlDataTransferred = this.dataSize;
                obj.SocketDataReceived = socketDataReceived;
                obj.TotalAdded = mAdded; //Needs new algorithm
                obj.TotalRemoved = mRemoved; //Needs new algorithm
                obj.ServerUptime = updateString;
                obj.SocketDataSent = this.socketDataSent;
                obj.TotalClientConnecting = totalClientConnected;
                obj.CpuTemp = this.cpuTemperature;
                obj.MotherboardTemp = this.moboTemperature;
                obj.TotalGcmMessages = this.gcmSent;

                JavaScriptSerializer s = new JavaScriptSerializer();
                String json = s.Serialize(obj);
                c.Send(json + "\n");
                return;

            }


            if (msg.ToLower() == App.CMD_REQUEST_SERVER_CHARTS)
            {
                ChartDataCollectionItem obj = new ChartDataCollectionItem();
                obj.Clients = clientsChartData;
                obj.Memory = MemoryChartData;
                obj.SocketReceived = SockReceivedChartData;
                obj.SocketSent = SockSentChartData;
                obj.Temp = TempChartData;

                JavaScriptSerializer s = new JavaScriptSerializer();
                String json = s.Serialize(obj);
                c.Send(json + "\n");
                return;
            }

            ResultItem data = serial.Deserialize<ResultItem>(msg);
            socketDataReceived += msg.Length;

            if (data.Request == App.CMD_REQUEST_DATA_STATUS)
            {
                long ms = (long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds;
                long diff = ms - LastUpdate;
                socketDataSent += SocketCommands.DataStatus(c, diff < AllowedLastUpdate);
                return;
            }

            if (data.Request == App.CMD_REQUEST_ACCEPT_DATA)
            {
                long  ms = (long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds;
                LastUpdate = ms;
                //Parse the incomming data!
                dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);
                
                IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                var dday = WatchData["data"] as IDictionary<string, object>;
                var hot = dday["HotMatches"] as Object[];
                var LR = dday["LatestResult"] as Object[];

                ParseMatchesFromSocket(hot, true);
                ParseMatchesFromSocket(LR, false);

                socketDataSent += SocketCommands.DataReceived(c, App.CMD_REQUEST_ACCEPT_DATA);
                return;
            }

            if (data.Request == App.CMD_REQUEST_ACCEPT_SB_STATUS) //Scorebot data. Maybe always assume that it has no scorebot and let the app only send message when it has scorebot. Less messages to handle i guess.
            {
                dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                var dday = WatchData["data"] as IDictionary<string, object>;
                int id = 0, t1 = 0, t2 = 0;
                bool sb = false;
                id = (int)dday["ID"];
                t1 = (int)dday["t1"];
                t2 = (int)dday["t2"];
                sb = (bool)dday["scoreBot"];

                foreach (Match m in hotMatches)
                {
                    if (m.ID == id)
                    {
                        m.TeamOne.ID = t1;
                        m.TeamTwo.ID = t2;
                        m.hasScorebot = sb;
                    }
                }


                socketDataSent += SocketCommands.DataReceived(c, App.CMD_REQUEST_ACCEPT_SB_STATUS);
                return;
            }


            MySQL sql = new MySQL();
            sql.CreateConnection("");
            totalClientConnected++;

            try
            { //Version 2.1 or newer
                if (data.Request == App.CMD_REQUEST_MATCHES)
                {
                    socketDataSent += SocketCommands.RequestedMatchesNew(c, data.app_version, hotMatches, latestResult);
                }

                if (data.Request == App.CMD_REQUEST_WATCH_STATUS)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String token = dday["token"] as string;
                    String id = dday["matchID"] as string;
                    socketDataSent += SocketCommands.WatchStatusNew(sql, c, id, token);
                }

                if (data.Request == App.CMD_REQUEST_REGISTER_GCM)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String token = dday["token"] as string;
                    String email = dday["email"] as string;
                    socketDataSent += SocketCommands.RegisterGCMNew(sql, c, email, token);
                }

                if (data.Request == App.CMD_REQUEST_UNREGISTER_WATCH)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String token = dday["token"] as string;
                    String id = dday["matchID"] as string;
                    socketDataSent += SocketCommands.UnregWatchNew(sql, c, id, token);
                }

                if (data.Request == App.CMD_REQUEST_REGISTER_WATCH)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String token = dday["token"] as string;
                    String id = dday["matchID"] as string;
                    socketDataSent += SocketCommands.RegWatchNew(sql, c, id, token);
                }

                if (data.Request == App.CMD_REQUEST_UNREGISTER_GCM)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String token = dday["token"] as string;
                    socketDataSent += SocketCommands.UnregGCMNew(sql, c, token);
                }

                if (data.Request == App.CMD_REQUEST_UPDATE_GCM_TOKEN)
                {
                    dynamic jsonData = new JavaScriptSerializer().DeserializeObject(msg);

                    IDictionary<string, object> WatchData = jsonData as IDictionary<string, object>;
                    var dday = WatchData["data"] as IDictionary<string, object>;
                    String oldToken = dday["oldToken"] as string;
                    String newToken = dday["newToken"] as string;
                    socketDataSent += SocketCommands.UpdateGcmTokenNew(sql, c, oldToken, newToken);
                }

            }
            catch (Exception e1)
            { // Version 2.0 or older

                //Matches
                if (msg.ToLower() == App.CMD_REQUEST_MATCHES)
                {
                    socketDataSent += SocketCommands.RequestedMatches(c, null, hotMatches, latestResult);
                }

                //Register GCM
                if (msg.ToLower().Contains(App.CMD_REQUEST_REGISTER_GCM))
                {
                    String[] key = msg.Replace(App.CMD_REQUEST_REGISTER_GCM + "=", "").Split('&');
                    socketDataSent += SocketCommands.RegisterGCM(sql, c, key[1], key[0]);
                }

                //Unregister GCM
                if (msg.ToLower().Contains(App.CMD_REQUEST_UNREGISTER_GCM))
                {
                    String key = msg.Replace(App.CMD_REQUEST_UNREGISTER_GCM + "=", "");
                    socketDataSent += SocketCommands.UnregGCM(sql, c, key);
                }

                //Register watch
                if (msg.ToLower().Contains(App.CMD_REQUEST_REGISTER_WATCH))
                {
                    String[] key = msg.Replace(App.CMD_REQUEST_REGISTER_WATCH + "=", "").Split('&');
                    socketDataSent += SocketCommands.RegWatch(sql, c, key[0], key[1]);
                }

                //Unregister watch
                if (msg.ToLower().Contains(App.CMD_REQUEST_UNREGISTER_WATCH))
                {
                    String[] key = msg.Replace(App.CMD_REQUEST_UNREGISTER_WATCH + "=", "").Split('&');
                    socketDataSent += SocketCommands.UnregWatch(sql, c, key[0], key[1]);
                }

                //Watch status
                if (msg.ToLower().Contains(App.CMD_REQUEST_WATCH_STATUS))
                {
                    String[] key = msg.Replace(App.CMD_REQUEST_WATCH_STATUS + "=", "").Split('&');
                    socketDataSent += SocketCommands.WatchStatusNew(sql, c, key[0], key[1]);
                }

                //Update gcm token
                if (msg.ToLower().Contains(App.CMD_REQUEST_UPDATE_GCM_TOKEN))
                {
                    String[] key = msg.Replace(App.CMD_REQUEST_UPDATE_GCM_TOKEN + "=", "").Split('&');
                    socketDataSent += SocketCommands.UpdateGcmToken(sql, c, key[0], key[1]);
                }
                


            }


            OnRssUpdated(null, null);
            sql.CloseConnection();
            
        }

        private void OnRssUpdated(object sender, RSSChangeEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate ()
            {
                lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
                lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
                lbsock.Content = "Sock: " + StringUtils.FormateFileSize(socketDataSent);
                lbClient.Content = "Total client: " + totalClientConnected;
                lbGcm.Content = "Total gcm: " + this.gcmSent;

            });
        }
        private void OnRssChanged(object sender, RSSChangeEventArgs e)
        {
            if (e.MatchRemoved)
            {
                RemovedUrls = e.RemovedUrls;
                latestUpdateTimer.Start();
                currentRecentUpdate = 0;
            }

            AddedUrls = e.AddedUrls;
            new Thread(GetHotMatches).Start();

            if (latestResult == null || latestResult.Count < 1)
            {
                GetLatestResult();
            }
            
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate ()
            {
                lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
                lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
                lbsock.Content = "Sock: " + StringUtils.FormateFileSize(socketDataSent);
                lbAdd.Content = "Total added: " + mAdded;
                lbRemove.Content = "Total removed: " + mRemoved;
                lbClient.Content = "Total client: " + totalClientConnected;
                lbGcm.Content = "Total gcm: " + this.gcmSent;
            });

        }

        private void ParseMatchesFromSocket(object[] data, bool hot)
        {
            List<Match> results = new List<Match>();
            foreach (var o in data)
            {
                var ob = (IDictionary<string, object>)o;

                int id = (int)ob["ID"];
                
                string url = ob.ContainsKey("Url") ? (string)ob["Url"] : "";
                string t = hot ? (string)ob["Time"] : "";
                bool over = (bool)ob["IsOver"];

                var t1 = (IDictionary<string, object>)ob["TeamOne"];
                var t2 = (IDictionary<string, object>)ob["TeamTwo"];
                Team team1, team2;
                if (!hot)
                {
                    string tos = (string)ob["tOneScore"];
                    string tts = (string)ob["tTwoScore"];
                    team1 = new Team((int)t1["id"], (string)t1["name"], (string)t1["country"], tos);
                    team2 = new Team((int)t2["id"], (string)t2["name"], (string)t2["country"], tts);
                } else
                {
                    team1 = new Team((int)t1["id"], (string)t1["name"], (string)t1["country"]);

                    team2 = new Team((int)t2["id"], (string)t2["name"], (string)t2["country"]);
                }

                if (!hot)
                {

                    id = url.Contains("/") ? int.Parse(url.Substring(url.LastIndexOf("/") + 1, 7)) : -1;
                }

                Match m = new Match(id, url, t, team1, team2, over, false);
                results.Add(m);
            }

            if (hot)
            {
                List<Match> known = new List<Match>();
                List<Match> removed = new List<Match>();
                List<Match> newMatches = new List<Match>();
                // known = results.Except(hotMatches).ToList(); //New matches

                //TODO: Test!
                for (int i = 0; i < hotMatches.Count; i++)
                {
                    if (!results.Contains(hotMatches[i]))
                    {
                        removed.Add(hotMatches[i]); //This is the matches that don't exists in the new list but did in the old = They have been removed
                    } else
                    {
                        known.Add(hotMatches[i]);
                        if (hotMatches[i].hasScorebot)
                            results[i].hasScorebot = true;
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    if (!known.Contains(results[i])) {
                        newMatches.Add(results[i]);
                    }
                }

                Console.WriteLine("New: " + newMatches.Count);

                mAdded += newMatches.Count;
                mRemoved += removed.Count;

                if (hotMatches != null && hotMatches.Count > 0 && newMatches.Count > 0)
                {
                    AddedUrls.Clear();
                    foreach (var m in newMatches)
                    {
                        AddedUrls.Add(m.Url);
                    }
                    
                }
                bool wasEmpty = hotMatches.Count < 1;

                hotMatches = results;

                if (newMatches.Count > 0 && !wasEmpty)
                {
                    CreateNewMatchGCM(AddedUrls);
                }

            } else
            {
                RemovedUrls.Clear();
                for (int i = 0; i < results.Count(); i++)
                {
                    if (!latestResult.Contains(results[i]))
                    {
                        RemovedUrls.Add(results[i].Url); //New results for finished matches!
                        Console.WriteLine(results[i].TeamOne.Name + " vs " + results[i].TeamTwo.Name + ") finished");
                        
                    }
                }

                bool wasEmpty = latestResult.Count < 1;


                latestResult = results;
                if (RemovedUrls.Count > 0 && !wasEmpty)
                {
                    new Thread(CreateMatchFinishedGCM).Start();
                }

               
            }



            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate ()
            {
                if (hot) {
                    lb_hot.ItemsSource = hotMatches;
                } else {
                    lb_result.ItemsSource = latestResult;
                }
                 lbHot.Content = "Hot matches: " + hotMatches.Count;
                 lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
                 lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
                 lbsock.Content = "Sock: " + StringUtils.FormateFileSize(socketDataSent);
                 lbClient.Content = "Total client: " + totalClientConnected;
                 lbGcm.Content = "Total gcm: " + this.gcmSent;
                lbAdd.Content = "Total added: " + mAdded;
                lbRemove.Content = "Total removed: " + mRemoved;

                ScorebotTimer.Start();
             });

        }

        private void CreateMatchFinishedGCM()
        {
            //if (App.isDebug)
            //{
            //    return;
            //}

            MySQL sql = new MySQL();
            sql.CreateConnection("");

            List<String> finished = RemovedUrls;
            foreach (var m in RemovedUrls)
            {
                String url = m;
                if (url == null)
                    return;

                String matchID = url.Substring(url.LastIndexOf("/") + 1, 7);
                Console.WriteLine("Sending GCM for: " + matchID);
                List<DBMatch> wMatches = sql.GetMatchesFromID(matchID);
                List<String> keys = new List<String>();
                foreach (var wm in wMatches)
                {
                    List<DBDevice> dev = sql.GetDevicesFromAccountID(wm.AccountID);
                    foreach (var s in dev)
                    {
                        keys.Add(s.Key);
                    }
                    
                }

                int offset = 0;
                while (offset < keys.Count)
                {
                    GCMObject obj = new GCMObject();
                    Match ma = null;
                    foreach (var lr in latestResult)
                    {
                        if (lr.ID == int.Parse(matchID))
                            ma = lr;
                    }
                    if (ma == null)
                        return;

                    obj.registration_ids.AddRange(keys.Skip(offset).Take(500));
                    obj.data.message = CreateWinnerNotificationText(ma);
                    obj.data.title = CreateWinnerTitle(ma);
                    obj.data.type = 1;

                    offset += 500;
                    GCMService.SendNotification(obj);
                    gcmSent++;
                }
                int result = sql.RemoveMatchesWithID(matchID);
                if (result == 1)
                {
                    Console.WriteLine("Match removed");
                } else
                {
                    Console.WriteLine("Unable to remove matches");
                }

            }
            RemovedUrls.Clear();
            sql.CloseConnection();
        }

        private String CreateWinnerTitle(Match m)
        {
            String text = "";
            if (m.TeamOne.ScoreHtml.Contains("#FF0000"))
            {
                text += m.TeamTwo.Name + " won";
            }

            if (m.TeamTwo.ScoreHtml.Contains("#FF0000"))
            {
                text += m.TeamOne.Name + " won";
            }

            if (m.TeamOne.ScoreHtml.Contains("#0000CC")) //Should have resulted in a equal-game (e.i 1:1 on a Bo2)
            {
                text = "It's a tie!";
            }

            return text;
        }

        private String CreateWinnerNotificationText(Match m)
        {


            String text = "";
            String scoreOne, scoreTwo;
            scoreOne = m.TeamOne.ScoreHtml.Replace("\n", "").Replace(" ", "");
            scoreOne = scoreOne.Substring(scoreOne.IndexOf(">") + 1, scoreOne.LastIndexOf("<") - (scoreOne.IndexOf(">") + 1));
            scoreTwo = m.TeamTwo.ScoreHtml.Replace("\n", "").Replace(" ", "");
            scoreTwo = scoreTwo.Substring(scoreTwo.IndexOf(">") + 1, scoreTwo.LastIndexOf("<") - (scoreTwo.IndexOf(">") + 1));
            if (m.TeamOne.ScoreHtml.Contains("#FF0000"))
            {
                text = "against " + m.TeamOne.Name + " with " + scoreTwo + "-" + scoreOne;
            }

            if (m.TeamTwo.ScoreHtml.Contains("#FF0000"))
            {
                text = "against " + m.TeamTwo.Name + " with " + scoreOne + "-" + scoreTwo;
            }

            if (m.TeamOne.ScoreHtml.Contains("#0000CC")) //Should have resulted in a equal-game (e.i 1:1 on a Bo2)
            {
                text = m.TeamOne.Name + " tied against " + m.TeamTwo.Name;
            }

            return text;
        }

        private void CreateNewMatchGCM(List<String> added)
        {
            //if (App.isDebug)
            //{
            //    return;
            //}

            int count = added.Count;
            GCMObject obj = new GCMObject();
            obj.data.title = "New matches!";
            obj.data.message = added.Count + " matches added!";
            obj.data.more = (added.Count - count);

            for (int i = 0; i < count; i++)
            {
              //  var m2 = hotMatches.Find(r => r.Url == added[i]);
                foreach (var m in hotMatches)
                {
                    if (m.Url == added[i])
                    {
                        String msg = "";
                        if (m != null)
                        {
                            //Team one
                            if (m.TeamOne != null)
                                msg += m.TeamOne.Name + " vs ";
                            else
                                msg += "TBD vs ";
                            //Team two
                            if (m.TeamTwo != null)
                                msg += m.TeamTwo.Name;
                            else
                                msg += "TBD";
                        }
                        else
                        {
                            msg = "TBD vs TBD";
                        }
                        obj.data.matches.Add(msg);
                    }
                }
              
                //Done
               
            }

            obj.data.type = 3;
            GCMService.SendNewMatchNoticifation(obj);
            gcmSent++;
        }

        private void GetHotMatches()
        {
            GetHotMatches(true);
        }


        private void GetHotMatches(bool sendNoti)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.hltv.org/?ref=logo");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null) {
                    readStream = new StreamReader(receiveStream);
                } else {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();
                dataSize += data.Length;
                response.Close();
                readStream.Close();


                Supremes.Nodes.Document doc = Supremes.Dcsoup.Parse(data);
                Supremes.Nodes.Elements el = doc.Select("li[id=boc1] div[class=vsbox]");
                Elements time = doc.Select("li[id=boc1] div[style*=position:relative;float:right;right:40px;]");
                List<Match> matches = new List<Match>();
                
                for (int main = 0; main < el.Count -1; main++) {
                    Match m = new Match();
                    m.Url = "http://www.hltv.org" + el[main].GetElementsByTag("a")[0].Attr("href").ToString();

                    Elements spans = el[main].GetElementsByTag("span");
                    Elements flags = el[main].GetElementsByTag("img");
                    Elements numbers = time[main].GetElementsByTag("div");

                    String timeString = "";
                    var parser = new Parser();
                    for (int n = 0; n < numbers.Count; n++) {


                        var rules = parser.Parse("element.style  { " + numbers[n].Attr("style") + ";}").StyleRules;
                        for (int r = 0; r < rules.Count; r++) {
                            var prop = rules[r].Declarations;
                            for (int p = 0; p < prop.Count; p++) {
                                if (prop[p].Term.ToString().Contains("url")) {
                                    String url = StringUtils.getURLFromCSS(prop[p].Term.ToString());
                                    int len = url.Length - ((url.LastIndexOf("/") + 1) + 4);
                                    url = url.Substring(url.LastIndexOf("/") + 1, len);
                                    timeString += url.Replace("colon", ":");
                                }
                            }
                        }
                    }
                    m.Time = timeString;
                    m.ID = int.Parse(m.Url.Substring(m.Url.LastIndexOf("/") + 1, 7));

                  //  int randomLive = new Random().Next(35);
                  //  m.hasScorebot = randomLive < 12 ? true : false;   //We are creating 'random' false live's

                    int count = 0;
                    for (int i = 0; i < spans.Count; i++) {
                        if (!spans[i].Attr("style").Contains("90%")) {
                            Team t = new Team();
                            t.Name = spans[i].Text;
                            try {
                                t.Country = StringUtils.getCountryCodeFromFlagUrl(flags[count].Attr("src"));
                            } catch (Exception e) { t.Country = "WORLD";  }
                            if (m.TeamOne == null) {
                                m.TeamOne = t;
                            } else {
                                m.TeamTwo = t;
                            }
                            count++;
                        }
                        
                    }
                    matches.Add(m);
                }
                
                hotMatches = matches;
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 (ThreadStart)delegate ()
                 {
                    lb_hot.ItemsSource = matches;
                     lbHot.Content = "Hot matches: " + matches.Count;
                     lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
                     lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
                     lbsock.Content = "Sock Sent: " + StringUtils.FormateFileSize(socketDataSent);
                     lbClient.Content = "Total client: " + totalClientConnected;
                     lbGcm.Content = "Total gcm: " + this.gcmSent;

                     ScorebotTimer.Start();
                 });
                if (sendNoti && (AddedUrls != null && AddedUrls.Count > 0))
                {
                    CreateNewMatchGCM(AddedUrls);
                }

            }


        }

        private void GetLatestResult()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.hltv.org/?ref=logo");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK) {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null) {
                    readStream = new StreamReader(receiveStream);
                } else {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();
                dataSize += data.Length;
                response.Close();
                readStream.Close();


                Supremes.Nodes.Document doc = Supremes.Dcsoup.Parse(data);
                Supremes.Nodes.Elements el = doc.Select("ul[id=secondCollumn] li[id=boc5] div[class=marker]");
                List<Match> matches = new List<Match>();
                foreach (Element result in el)
                {
                    Match m = new Match();
                    m.IsOver = true; //Latest result will also containt matches that have ended

                    Elements teams = result.Select("div[class=teams] div[class=shorten]");
                    Elements links = result.Select("div[class=extra] a[href*=/match/]");

                    foreach (Element link in links) {
                        m.Url = "http://www.hltv.org" + link.Attr("href");
                    }

                    Team t1 = new Team();
                    t1.Name = teams[0].Text;
                    t1.ScoreHtml = teams[0].NextSibling.ToString();
                    t1.Country = StringUtils.getCountryCodeFromFlagUrl(teams[0].GetElementsByTag("img").First.Attr("src"));
                    m.TeamOne = t1;


                    Team t2 = new Team();
                    t2.Name = teams[1].Text;
                    t2.ScoreHtml = teams[1].NextSibling.ToString();
                    t2.Country = StringUtils.getCountryCodeFromFlagUrl(teams[1].GetElementsByTag("img").First.Attr("src"));
                    m.TeamTwo = t2;
                    matches.Add(m);
                }
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 (ThreadStart)delegate ()
                 {
                     lb_result.ItemsSource = matches;
                     latestResult = matches;
                     lbSize.Content = "Total: " + StringUtils.FormateFileSize(dataSize + socketDataReceived + socketDataSent);
                     lbrss.Content = "Sock Rec: " + StringUtils.FormateFileSize(socketDataReceived);
                     lbsock.Content = "Sock Sent: " + StringUtils.FormateFileSize(socketDataSent);
                     lbClient.Content = "Total client: " + totalClientConnected;
                     lbGcm.Content = "Total gcm: " + this.gcmSent;
                 });

            }
        }


        private void FixDataPoints()
        {

            if (SockSentChartData.Count > maxDataPoints)
            {
                while (SockSentChartData.Count > maxDataPoints)
                {
                    SockSentChartData.RemoveAt(0);
                }
            }

            if (MemoryChartData.Count > maxDataPoints)
            {
                while (MemoryChartData.Count > maxDataPoints)
                {
                    MemoryChartData.RemoveAt(0);
                }
            }


            if (SockReceivedChartData.Count > maxDataPoints)
            {
                while (SockReceivedChartData.Count > maxDataPoints)
                {
                    SockReceivedChartData.RemoveAt(0);
                }
            }


            if (clientsChartData.Count > maxDataPoints)
            {
                while (clientsChartData.Count > maxDataPoints)
                {
                    clientsChartData.RemoveAt(0);
                }
            }

            if (TempChartData.Count > maxDataPoints)
            {
                while (TempChartData.Count > maxDataPoints)
                {
                    TempChartData.RemoveAt(0);
                }
            }


        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ConfigCollection cfg = new ConfigCollection();
            //cfg.Added = RSS.TotalAdded;
            //cfg.Removed = RSS.TotalRemoved;
            //Serialization.SaveConfig(cfg);

            server.Stop();       
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Loaded in " + (App.isDebug ? "debug" : "release") + " mode!");
            //ConfigCollection cfg = Serialization.LoadConfig();
            stopwatch = new Stopwatch();



            new Thread(initScorebot).Start();

            temp = new TempMonitor();
            temp.OnUpdateConnected += TempUpdate;
            temp.Init();

            server = new TCPServer();
            server.OnNewClientConnected += new TCPServer.NewClientHandler(ClientConnected);
            new Thread(server.Start).Start();


           // RSS = new RSS_Service(0, 0);
           // RSS.OnRssFeedChanged += new RSS_Service.FeedChangeHandler(OnRssChanged);
           // RSS.OnRssUpdate += new RSS_Service.FeedChangeHandler(OnRssUpdated);
           // RSS.Start();

            latestUpdateTimer.Tick += new EventHandler(latestUpdateTimer_Tick);
            latestUpdateTimer.Interval = new TimeSpan(0, 0, 30);

            RunningTimer.Tick += new EventHandler(RunningTimer_Tick);
            RunningTimer.Interval = new TimeSpan(0, 0, 1);
            RunningTimer.Start();

            ChartTimer.Tick += new EventHandler(ChartTimer_Tick);
            ChartTimer.Interval = new TimeSpan(0, 10, 0);
            ChartTimer.Start();

            ScorebotTimer.Tick += new EventHandler(Scorebot_tick);
            ScorebotTimer.Interval = new TimeSpan(0, 3, 0);
            


        }

        public void initScorebot()
        {
            scorebot = new ScorebotService("http://scorebot2.hltv.org");
            scorebot.OnStatusUpdateConnected += new ScorebotService.UpdatetHandler(ScorebotStatusUpdate);
        }

        private void ScorebotStatusUpdate(object sender, ScorebotUpdateEventArgs e)
        {
            hotMatches = e.ScorebotStatusUpdate;


            ResultItem ri = new ResultItem();
            MatchCollectionItem obj = new MatchCollectionItem();

            obj.HotMatches = hotMatches;
            obj.LatestResults = latestResult;
            ri.Request = App.CMD_REQUEST_MATCHES;
            ri.Data = obj;

            JavaScriptSerializer serial = new JavaScriptSerializer();
            String json = serial.Serialize(ri);
        }

        private void Scorebot_tick(object sender, EventArgs e)
        {
            ThreadStart childref = new ThreadStart(UpdateScorebotStatus);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }

        private void UpdateScorebotStatus()
        {
            scorebot.Connect(hotMatches);
        }

        private void TempUpdate(object sender, EventArgs e)
        {
            if (!temp.isLoaded)
                return;

            PerformanceCounter performanceCounter = new PerformanceCounter();
            performanceCounter.CategoryName = "Process";
            performanceCounter.CounterName = "Working Set";
            performanceCounter.InstanceName = Process.GetCurrentProcess().ProcessName;

            memoryCollection.Add((long)performanceCounter.NextValue());

            IEnumerable<ISensor> cpu = temp.CPU.Sensors.Where(o => o.SensorType == SensorType.Temperature);
            IEnumerable<ISensor> mobo = temp.Motherboard.Sensors.Where(o => o.SensorType == SensorType.Temperature);

            this.cpuTemp = float.Parse(cpu.First().Value.ToString());
            String tempC = Math.Round(double.Parse(cpuTemp + ""), 1) + "";
            tempC = tempC.Substring(0, tempC.IndexOf(",") > -1 ? tempC.IndexOf(",") + 2 : tempC.Length);
            cpuTemperature = tempC;

            String TempM = mobo.First().Value.ToString();
            TempM = TempM.Substring(0, TempM.IndexOf(",") > -1 ? TempM.IndexOf(",") + 2 : TempM.Length);
            moboTemperature = TempM;

            lbTemp.Content = "Temp: Cpu: " + tempC + "°C - Mobo: " + TempM + "°C";
        }


        private void ChartTimer_Tick(object sender, EventArgs e)
        {

            long recentSocketSent = socketDataSent - lastSocketSent;
            long recentSocketReceived = socketDataReceived - lastSocketReceived;
            long recentClient = totalClientConnected - lastClient;

            lastHtml = dataSize;
            lastSocketSent = socketDataSent;
            lastSocketReceived = socketDataReceived;
            lastClient = totalClientConnected;

            long mem = 0;
            foreach (long l in memoryCollection)
            {
                mem += l;
            }

            string time = DateTime.Now.ToString("MM/dd HH:mm");

            TempChartData.Add(cpuTemp);
            SockSentChartData.Add(new CharItemLong(recentSocketSent, time));
            MemoryChartData.Add(mem / memoryCollection.Count);
            SockReceivedChartData.Add(recentSocketReceived);
            clientsChartData.Add(new CharItemLong(recentClient, time));

            memoryCollection.Clear();

            new Thread(FixDataPoints).Start();

        }

        private void RunningTimer_Tick(object sender, EventArgs e)
        {
            var delta = DateTime.Now - startTime;
            long serverUptimeSeconds = (long)delta.TotalMilliseconds / 1000;

            updateString =
           String.Format("{0} d {1} H {2} M {3} s",
           (serverUptimeSeconds / 86400),
           ((serverUptimeSeconds % 86400) / 3600),
           (((serverUptimeSeconds % 86400) % 3600) / 60),
           (((serverUptimeSeconds % 86400) % 3600) % 60)
           );

            label2.Content = updateString;


        }

        private void latestUpdateTimer_Tick(object sender, EventArgs e)
        {
            new Thread(GetLatestResult).Start();
            if (RemovedUrls == null || RemovedUrls.Count < 1)
            {
                latestUpdateTimer.Stop();
                return;
            }
            foreach (var s in RemovedUrls)
            {
                foreach (var m in latestResult)
                {
                    if (m.Url != null)
                    {
                        try {
                            if (m.Url.Equals(s))
                            {
                                latestUpdateTimer.Stop();
                                new Thread(CreateMatchFinishedGCM).Start();
                                return;
                            }
                        } catch (Exception e1) { latestUpdateTimer.Stop(); return; }
                    }
                }
            }
            if (currentRecentUpdate >= maxRecentUpdate)
            {
                latestUpdateTimer.Stop();
            }  else
            {
                currentRecentUpdate++;
            }

        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Reset();
            stopwatch.Start();
            GetHotMatches(false);
            stopwatch.Stop();

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Reset();
            stopwatch.Start();
            GetLatestResult();
            stopwatch.Stop();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            new GcmWindow().Show();
        }
    }



}
