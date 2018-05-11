using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NeuralNetworkMaster
{
    public class TcpHole
    {
        public TcpClient client { get; set; }
        public int Count = 0;
        public bool Success = false;
        public void Connect(IPEndPoint peerRemoteEndPoint)
        {
           
            while (true)
            {
                try
                {
                    client.Connect(peerRemoteEndPoint);
                    Success = true;
                    break;
                }
                catch (Exception e)
                {
                    if (Count <= 3)
                    {
                        Count++;
                        continue;
                    }else
                    {
                        break;
                    }
                }
            }
        }

        public TcpClient PunchHole(IPEndPoint localEndPoint ,IPEndPoint peerRemoteEndPoint)
        {
            client = new TcpClient(localEndPoint);
            Connect(peerRemoteEndPoint);
            return client;
        }
    }
}