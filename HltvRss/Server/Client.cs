using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HltvRss.Server
{
    class Client
    {

        public delegate void NewClientHandler(object sender, ClientMsgReceivedEventArgs e);
        public event NewClientHandler OnClientReceive;

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        private Socket mSock;
        public Client()
        {
            
        }

        public void Send(String json)
        {
            
            byte[] byteData = Encoding.ASCII.GetBytes(json);
            mSock.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), mSock);
            
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //   handler.Shutdown(SocketShutdown.Both);
                //   handler.Close();

                //For this app, we don't need to start receiving after we sent the response
                // StartReceive();
                Thread.Sleep(100);
                handler.Dispose();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                  //  Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                   //     content.Length, content);

                    if (this.OnClientReceive != null)
                    {
                        ClientMsgReceivedEventArgs args = new ClientMsgReceivedEventArgs(this, state);
                        this.OnClientReceive(this, args);
                    }
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
          

        }

        private void StartReceive()
        {
            StateObject state = new StateObject();
            state.workSocket = mSock;
            byte[] buffer = new byte[16384];
            mSock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,new AsyncCallback(ReadCallback), state);
        }

        public Socket ClientSocket
        {
            get
            {
                return mSock;
            }
            set
            {
                mSock = value;
                StartReceive();
            }
        }
        
    }
}
