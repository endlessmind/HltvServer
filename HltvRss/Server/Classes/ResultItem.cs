using System;

namespace HltvRss.Classes
{
    class ResultItem
    {
        private String mRequest;
        private String mVersion;
        private Object mData;

        public String app_version
        {
            get
            {
                return mVersion;
            }
            set
            {
                mVersion = value;
            }
        }

        public String Request
        {
            get
            {
                return mRequest;
            }
            set
            {
                mRequest = value;
            }
        }

        public Object Data
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


    }
}
