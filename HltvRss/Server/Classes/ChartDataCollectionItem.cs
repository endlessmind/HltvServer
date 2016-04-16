using System.Collections.Generic;

namespace HltvRss.Server.Classes
{
    class ChartDataCollectionItem
    {
        List<CharItemLong> clientsChartData = new List<CharItemLong>();
        List<CharItemLong> SockSentChartData = new List<CharItemLong>();
        List<long> memoryChartData = new List<long>();
        List<long> SockReceivedChartData = new List<long>();
        List<float> tempChartData = new List<float>();

        public List<float> Temp
        {
            get
            {
                return tempChartData;
            }
            set
            {
                tempChartData = value;
            }
        }

        public List<CharItemLong> Clients
        {
            get
            {
                return clientsChartData;
            }
            set
            {
                clientsChartData = value;
            }
        }

        public List<CharItemLong> SocketSent
        {
            get
            {
                return SockSentChartData;
            }
            set
            {
                SockSentChartData = value;
            }
        }

        public List<long> Memory
        {
            get
            {
                return memoryChartData;
            }
            set
            {
                memoryChartData = value;
            }
        }

        public List<long> SocketReceived
        {
            get
            {
                return SockReceivedChartData;
            }
            set
            {
                SockReceivedChartData = value;
            }
        }

    }
}
