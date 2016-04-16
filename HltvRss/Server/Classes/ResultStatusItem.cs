using System;

namespace HltvRss.Classes
{
    class ResultStatusItem
    {
        private String mValue;

        public String Status
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
    }
}
