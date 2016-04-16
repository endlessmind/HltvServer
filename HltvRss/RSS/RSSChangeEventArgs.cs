using System;
using System.Collections.Generic;

namespace HltvRss.RSS
{
    class RSSChangeEventArgs : EventArgs
    {

        public bool MatchRemoved { get; private set; }
        public List<String> RemovedUrls { get; private set; }
        public List<String> AddedUrls { get; private set; }
        public int AddedCount { get; private set; }

        public RSSChangeEventArgs(List<String> removedUrls, List<String> addedUrls, bool remove, int added)
        {
            MatchRemoved = remove;
            RemovedUrls = removedUrls;
            AddedUrls = addedUrls;
            AddedCount = added;
        }
    }
}
