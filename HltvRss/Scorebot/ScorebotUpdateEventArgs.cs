using HltvRss.Classes;
using System;
using System.Collections.Generic;

namespace HltvRss.Scorebot
{
    class ScorebotUpdateEventArgs : EventArgs
    {
        public List<Match> ScorebotStatusUpdate { get; private set; }

        public ScorebotUpdateEventArgs(List<Match> m)
        {
            ScorebotStatusUpdate = m;
        }
    }
}
