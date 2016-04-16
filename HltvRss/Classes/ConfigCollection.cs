using System;

namespace HltvRss.Classes
{
    [Serializable]
    class ConfigCollection 
    {
        private long mAdded;
        private long mRemoved;
        private long mNewRegisterd;

        public ConfigCollection()
        {
            mAdded = 0;
            mRemoved = 0;
            mNewRegisterd = 0;
        }

        public long Added
        {
            get
            {
                return mAdded;
            }
            set
            {
                mAdded = value;
            }
        }

        public long Removed
        {
            get
            {
                return mRemoved;
            }
            set
            {
                mRemoved = value;
            }
        }

        public long NewRegister
        {
            get
            {
                return mNewRegisterd;
            }
            set
            {
                mNewRegisterd = value;
            }
        }
    }
}
