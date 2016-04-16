using System;

namespace HltvRss.Server
{
    class ClientConnectEventArgs : EventArgs
    {
        public Client ConnectedClient { get; private set; }

        public ClientConnectEventArgs(Client c)
        {
            ConnectedClient = c;
        }
    }
}
