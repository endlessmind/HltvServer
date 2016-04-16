using HltvRss.Classes;
using HltvRss.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Linq;

namespace HltvRss.RSS
{
    class RSS_Service
    {

        public delegate void FeedChangeHandler(object sender, RSSChangeEventArgs e);
        public event FeedChangeHandler OnRssFeedChanged;
        public event FeedChangeHandler OnRssUpdate;

        DispatcherTimer rssUpdateTimer = new DispatcherTimer();
        List<String> rssMatches = new List<String>();
        //List<String> rssHash = new List<String>();

        List<RSSItem> rrsItems = new List<RSSItem>();

        private long mAdded;
        private long mRemoved;

        private bool firstRun = false;

        private long mDataSize;

        public RSS_Service(long added, long removed)
        {
            rssUpdateTimer.Tick += new EventHandler(rssUpdateTimer_Tick);
            rssUpdateTimer.Interval = new TimeSpan(0, 1, 0);
            mAdded = added;
            mRemoved = removed;
        }

        public long TotalAdded
        {
            get
            {
                return mAdded;
            }
        }

        public long TotalRemoved
        {
            get
            {
                return mRemoved;
            }
        }

        public long DataTransfered
        {
            get
            {
                return mDataSize;
            }
        }

        public void Start()
        {
            rssUpdateTimer.Start();
            firstRun = true;
            new Thread(GetRss).Start();
        }

        public void Stop()
        {
            rssUpdateTimer.Stop();
        }

        private void rssUpdateTimer_Tick(object sender, EventArgs e)
        {
            new Thread(GetRss).Start();
        }

        //Used to detect when a match as been added(upcoming) or removed(finished). 
        //That would trigger it to update the json-data available for the app to request.
        //
        //The actually match-pages will still be loaded and parsed by the Android-app it self.
        private void GetRss()
        {

            using (WebClient client = new WebClient())
            {
                string xmlData;
                try
                {
                    xmlData = client.DownloadString("http://www.hltv.org/hltv.rss.php");
                } catch (Exception e) { return; /*We're just ingnoring any errors */ }

                mDataSize += xmlData.Length;
                XDocument doc = XDocument.Parse(xmlData);

                var items = doc.Descendants("item");
                List<String> matches = new List<String>();
                //List<String> hashes = new List<String>();
                List<RSSItem> rrs = new List<RSSItem>();
                foreach (var item in items.Take(40))
                {
                    RSSItem itm = new RSSItem();

                    String date = item.Descendants("pubDate").ToList<XElement>()[0].Value;
                    String url = item.Descendants("link").ToList<XElement>()[0].Value;
                    itm.Url = url;
                    itm.ID = int.Parse(url.Substring(url.LastIndexOf("/") +1, 7));
                    itm.Date = date;
                    itm.Hash = StringUtils.CreateMD5(url + date);

                    matches.Add(url);
                    rrs.Add(itm);
                }
                //Any changes to the last rss?
                var removed = rssMatches.Except(matches).ToList(); // Found in the old rss but not in the new (Finished matches)
                var added = matches.Except(rssMatches).ToList(); //Found it the new rss but not in the old (Newly added)
                int addedCount = 0;
                bool add = false;
                bool remove = false;
                bool edit = false;
                List<String> RemovedUrls = new List<String>();
                List<String> AddedUrls = new List<String>();
                if (!firstRun)
                {


                    List<int> AddedID = new List<int>();
                    //Run the add-loop first
                    foreach (var m in added)
                    {
                        
                        int mID = int.Parse(m.Substring(m.LastIndexOf("/") + 1, 7));
                        AddedID.Add(mID);
                        bool itemEdit = false;
                        foreach (var itm in rrsItems)
                        {
                            if (mID == itm.ID) {
                                itemEdit = true;
                            }
                        }
                        
                        if (!itemEdit) {
                            mAdded++;
                            addedCount++;
                            AddedUrls.Add(m);
                        } 
                        add = true; //We still want the json-data to update (maybe we got new url or new time)
                    }

                    foreach (var m in removed)
                    {
                        
                        int mID = int.Parse(m.Substring(m.LastIndexOf("/") + 1, 7));
                        bool itemEdit = false;
                        foreach (var i in AddedID)
                        {
                            if (i == mID)
                            {
                                itemEdit = true;
                                edit = true;
                            }
                        }
                        if (!itemEdit)
                        {
                            RemovedUrls.Add(m);
                            remove = true;
                            mRemoved++;
                        }

                    }
                    

                    //var newHashes = rssHash.Except(hashes).ToList();
                    //if (newHashes != null && newHashes.Count > 0)
                    //{
                    //    add = true; //Update the json-data
                    //    //foreach (var h in newHashes) //Maybe if we want to specify to the users that a match has been moved. But not right now!
                    //    //{
                    //    //    //int id = hashes.IndexOf(h);
                    //    //    Console.WriteLine("bla: " + h);
                    //    //}
                    //}
                } else
                {
                    firstRun = false;
                    remove = true; //Trigger both list to update!
                    if (mAdded == 0)
                        mAdded = matches.Count;
                }

                //Should find out of time has changed?
                foreach (var r in rrs)
                {
                    var oRL = from rr in rrsItems where rr.ID == r.ID select rr;

                    if (oRL != null && oRL.Any())
                    {
                        var oR = oRL.First();
                         if (((RSSItem)oR).Date != r.Date)
                        {
                            //Date changed
                            edit = true;
                        }
                    }
                }

                //If there is 0 items in the rss-feed, we should update the list.
                //We need this as it wont detectigt while looping as there will be no items in the matches-array if the rrs is empty
                //so the loop wont actually run!
                if (rssMatches.Count > 0 && matches.Count == 0)
                {
                    remove = true;
                }

                RSSChangeEventArgs args = new RSSChangeEventArgs(RemovedUrls, AddedUrls, remove, addedCount);
                if (add || remove || edit)
                {
                    if (OnRssFeedChanged == null) return;

                    
                    OnRssFeedChanged(this, args);
                }
                OnRssUpdate(this,args);
                rssMatches = matches;
                //rssHash = hashes;
                rrsItems = rrs;
            }
        }

    }
}
