using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NeuralNetworkMaster.Model;
using System.Net;
using System.Linq;

namespace NeuralNetworkMaster.Communication
{
    class CommunicationsLayer
    {
        public CommunicationModule server { get; set; }
        public string PipelineId { get; set; }
        public CommunicationsLayer()
        {
            server = new CommunicationModule("192.168.1.2", 6000);
        }

        public void SendCommunicationServerParameters()
        {
            server.SendData(JsonConvert.SerializeObject(new CommunicationServerParameters()
            {
                PipelineId = PipelineId,
                IsMaster = true
            }));

        }

        public IPEndPoint GetPeerIPEndPoint()
        {
            return GetIpEndPoint(server.ReceiveData());
        }


        public  IPEndPoint GetIpEndPoint(string ipEndPointString)
        {
            var localEndPointList = ipEndPointString.Split(':');
            var ipAddress = localEndPointList[0].Split('.').Select(i => Convert.ToByte(i)).ToArray();
            return new IPEndPoint(new IPAddress(ipAddress), int.Parse(localEndPointList[1]));
        }
    }
}
