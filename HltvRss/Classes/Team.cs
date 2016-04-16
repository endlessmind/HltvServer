using System;

namespace HltvRss.Classes
{

    class Team : IEquatable<Team>
    {

        private String mName;
        private String mCountry;
        private String mScore;
        private int mID = -1;

        public Team()
        {
        }

        public Team(int id, string name, string country)
        {
            mID = id;
            mName = name;
            mCountry = country;
        }

        public Team(int id, string name, string country, string score)
        {
            mID = id;
            mName = name;
            mCountry = country;
            mScore = score;
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


        public String Text
        {
            get
            {
                return mName + "(" + mCountry + ")";
            }
        }

        public String ScoreHtml
        {
            get
            {
                return mScore;
            }
            set
            {
                mScore = value;
            }
        }

        public String Name
        {
            get
            {
                return mName;
            }
             set
            {
                mName = value;
            }
        }

        public String Country
        {
            get
            {
                return mCountry;
            }
            set
            {
                mCountry = value;
            }
        }

        public bool Equals(Team other)
        {
            return this.Name == other.Name && this.Country == other.Country;
        }
    }
}
