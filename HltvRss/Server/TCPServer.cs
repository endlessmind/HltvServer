using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HltvRss.Server
{
    class TCPServer
    {
        
         List<Client> connectedClients = new List<Client>();

        public delegate void NewClientHandler(object sender, ClientConnectEventArgs e);
        public event NewClientHandler OnNewClientConnected;

        Socket serverSock;
        bool listening = false;
        public static ManualResetEvent allDone = new ManualResetEvent(false);



        public TCPServer()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipep = new IPEndPoint(ipAddress, 9060);

            serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(ipep);
           
        }

        public void Start()
        {
            serverSock.Listen(100);
            listening = true;
            while (listening)
            {
                allDone.Reset();
                serverSock.BeginAccept(Accept, serverSock);
                allDone.WaitOne();
            }
            
            
        }

        public void Stop()
        {
            listening = false;
            allDone.Set();
            serverSock.Close();

            Console.WriteLine("Server is stoped!");
        }

        private void Accept(IAsyncResult ar)
        {
            if (!listening)
                return;

            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Client c = new Client();
            c.ClientSocket = handler;

            

            if (OnNewClientConnected == null) return;
                ClientConnectEventArgs args = new ClientConnectEventArgs(c);
                OnNewClientConnected(this, args);

        }
    }
}
