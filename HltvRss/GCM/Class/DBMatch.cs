namespace HltvRss.GCM.Class
{
    class DBMatch
    {
        private int mID;
        private int mMatchID;
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

        public int MatchID
        {
            get
            {
                return mMatchID;
            }
            set
            {
                mMatchID = value;
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
