using System;
using System.Collections.Generic;

namespace HltvRss.Classes
{
    class ScorebotItem
    {
        private int mID;
        private Boolean Live;
        private List<int> mtIDs;


        public List<int> TeamIDs
        {
            get
            {
                return mtIDs;
            }
            set
            {
                mtIDs = value;
            }
        }


        public override String ToString()
        {
            return " ID: " + ID + " - Live: " + isLive;
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


        public Boolean isLive
        {
            get
            {
                return Live;
            }
            set
            {
                Live = value;
            }
        }

    }
}
