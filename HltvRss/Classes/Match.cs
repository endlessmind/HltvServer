using System;

namespace HltvRss.Classes
{
    class Match : IEquatable<Match>
    {
        private Team mTeamOne;
        private Team mTeamTwo;
        private String mUrl;
        private String time;
        private Boolean isOver;
        private Boolean hasBot = false; //if ture, then the scorebot is on and live
        private int mID;


        public Match()
        {

        }

        public Match(int id, string url, string t, Team t1, Team t2, bool over, bool score)
        {
            mID = id;
            mUrl = url;
            time = t;
            mTeamOne = t1;
            mTeamTwo = t2;
            IsOver = over;
            hasBot = score;
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

        public override string ToString()
        {

            return mTeamOne.Name + "(" + mTeamOne.Country + ") vs " + mTeamTwo.Name + "(" + mTeamTwo.Country + ")";

        }

        bool IEquatable<Match>.Equals(Match other)
        {
            return this.ID == other.ID;
        }

        public Team TeamOne
        {
            get
            {
                return mTeamOne;
            }
            set
            {
                mTeamOne = value;
            }
        }

        public Team TeamTwo
        {
            get
            {
                return mTeamTwo;
            }
            set
            {
                mTeamTwo = value;
            }
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

        public String Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }

        public Boolean IsOver
        {
            get
            {
                return isOver;
            }
            set
            {
                isOver = value;
            }
        }

        public Boolean hasScorebot
        {
            get
            {
                return hasBot;
            }
            set
            {
                hasBot = value;
            }
        }

    }
}
