using System;
using System.Collections.Generic;

namespace HltvRss.GCM.Class
{
    [Serializable]
    class GCMObject
    {
        private List<String> mRegistration_ids = new List<string>();
        private Data mData = new Data();

        public List<String> registration_ids
        {
            get
            {
                return mRegistration_ids;
            }
            set
            {
                mRegistration_ids = value;
            }
        }

        public Data data
        {
            get
            {
                return mData;
            }
            set
            {
                mData = value;
            }
        }

        [Serializable]
        public class Data
        {
            private String msg;
            private String titl;
            private List<String> mMatches = new List<string>();
            private int mMore;
            private int mType;

            public int more
            {
                get
                {
                    return mMore;
                }
                set
                {
                    mMore = value;
                }
            }

            public String message
            {
                get
                {
                    return msg;
                }
                set
                {
                    msg = value;
                }
            }

            public String title
            {
                get
                {
                    return titl;
                }
                set
                {
                    titl = value;
                }
            }

            public List<String> matches
            {
                get
                {
                    return mMatches;
                }
                set
                {
                    mMatches = value;
                }
            }

            public int type
            {
                get
                {
                    return mType;
                }
                set
                {
                    mType = value;
                }
            }
        }
    }
}
