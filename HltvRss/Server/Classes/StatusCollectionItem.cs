using System;
using System.Diagnostics;

namespace HltvRss.Classes
{
    class StatusCollectionItem
    {
        private long mHtmlDataSize;
        private long mRssDataSize;
        private long mSocketDataSize;
        private int mHotCount; //Don't need recent as it's allways 10
        private long mTotalMatchesAdded;
        private long mTotalMatchesRemoved;
        private long mClientConnected;
        private long mGcmSent;
        private String mCpuTemp;
        private String mMoboTemp;
        private String uptime;

        public String CpuTemp
        {
            get
            {
                return mCpuTemp;
            }
            set
            {
                mCpuTemp = value;
            }
        }

        public String MotherboardTemp
        {
            get
            {
                return mMoboTemp;
            }
            set
            {
                mMoboTemp = value;
            }
        }

        public String ServerUptime
        {
            get
            {
                return uptime;
            }
            set
            {
                uptime = value;
            }
        }

        public long ServerMemoryUsage
        {
            get
            {
                PerformanceCounter performanceCounter = new PerformanceCounter();
                performanceCounter.CategoryName = "Process";
                performanceCounter.CounterName = "Working Set";
                performanceCounter.InstanceName = Process.GetCurrentProcess().ProcessName;

                return (long)performanceCounter.NextValue();
            }
        }

        public long TotalGcmMessages
        {
            get
            {
                return mGcmSent;
            }
            set
            {
                mGcmSent = value;
            }
        }

        public long TotalClientConnecting
        {
            get
            {
                return mClientConnected;
            }
            set
            {
                mClientConnected = value;
            }
        }

        public long SocketDataSent
        {
            get
            {
                return mSocketDataSize;
            }
            set
            {
                mSocketDataSize = value;
            }
        }

        public long HtmlDataTransferred
        {
            get
            {
                return this.mHtmlDataSize;
            }
            set
            {
                this.mHtmlDataSize = value;
            }
        }

        public long SocketDataReceived
        {
            get
            {
                return this.mRssDataSize;
            }
            set
            {
                this.mRssDataSize = value;
            }
        }

        public int HotMatchesCount
        {
            get
            {
                return this.mHotCount;
            }
            set
            {
                this.mHotCount = value;
            }
        }

        public long TotalAdded
        {
            get
            {
                return this.mTotalMatchesAdded;
            }
            set
            {
                this.mTotalMatchesAdded = value;
            }
        }

        public long TotalRemoved
        {
            get
            {
                return this.mTotalMatchesRemoved;
            }
            set
            {
                this.mTotalMatchesRemoved = value;
            }
        }

    }
}
