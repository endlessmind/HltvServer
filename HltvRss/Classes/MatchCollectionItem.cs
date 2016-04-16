using System.Collections.Generic;

namespace HltvRss.Classes
{
    class MatchCollectionItem
    {
        private List<Match> hot;
        private List<Match> recent;

        public List<Match> HotMatches
        {
            get
            {
                return hot;
            }
            set
            {
                hot = value;
            }
        }

        public List<Match> LatestResults
        {
            get
            {
                return recent;
            }
            set
            {
                recent = value;
            }
        }
    }
}
