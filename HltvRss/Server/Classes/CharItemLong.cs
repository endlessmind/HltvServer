using System;

namespace HltvRss.Server.Classes
{
    class CharItemLong
    {
        private long mValue;
        private string mTime;


        public CharItemLong()
        {

        }

        public CharItemLong(long value, string time)
        {
            mValue = value;
            mTime = time;
        }


        public long Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }


        public String Time
        {
            get
            {
                return mTime;
            }
            set
            {
                mTime = value;
            }
        }

    }
}
