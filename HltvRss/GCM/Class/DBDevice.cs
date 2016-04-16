using System;

namespace HltvRss.GCM.Class
{
    class DBDevice
    {
        private int mID;
        private String mKey;
        private int mAccountID;

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


        public String Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }

        public int AccountID
        {
            get
            {
                return mAccountID;
            }
            set
            {
                mAccountID = value;
            }
        }


    }
}
