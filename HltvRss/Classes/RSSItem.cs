using System;

namespace HltvRss.Classes
{
    class RSSItem
    {
        private String mUrl;
        private String mHash;
        private String mDate;
        private int mID;

        public RSSItem()
        {

        }

        public String Url
        {
            get
            {
                return mUrl;
            }
            set
            {
                mUrl = value;
            }
        }

        public String Hash
        {
            get
            {
                return mHash;
            }
            set
            {
                mHash = value;
            }
        }

        public String Date
        {
            get
            {
                return mDate;
            }
            set
            {
                mDate = value;
            }
        }

        public int ID
        {
            get
            {
                return mID;
            }
            set
            {
                mID = value;
            }
        }
    }
}
