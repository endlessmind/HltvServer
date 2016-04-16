using System;

namespace HltvRss.GCM.Class
{
    class DBAccount
    {
        private int mID;
        private String mEmail;
        private int mNewMatchNoti;

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

        public String Email
        {
            get
            {
                return mEmail;
            }
            set
            {
                mEmail = value;
            }
        }

        public int WantNewMatchNotification
        {
            get
            {
                return mNewMatchNoti;
            }
            set
            {
                mNewMatchNoti = value;
            }
        }

    }
}
