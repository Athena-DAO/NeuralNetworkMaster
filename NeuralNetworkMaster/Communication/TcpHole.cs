using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NeuralNetworkMaster
{
    public class TcpHole
    {
        public TcpClient client { get; set; }
        public void Connect(IPEndPoint peerRemoteEndPoint)
        {
           
            while (true)
            {
                try
                {
                    client.Connect(peerRemoteEndPoint);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt {0}", e);
                    continue;
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