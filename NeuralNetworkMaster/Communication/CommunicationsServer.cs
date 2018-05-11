using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NeuralNetworkMaster.Model;
using System.Net;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NeuralNetworkMaster.Communication
{
    class CommunicationsServer
    {
        public CommunicationTcp server { get; set; }
        public string PipelineId { get; set; }

        
        public CommunicationsServer(IConfiguration Configuration)
        {
            server = new CommunicationTcp($"{Configuration["ip-communication-server"]}", int.Parse($"{Configuration["port-communication-server"]}"));
        }

       
        public void SendCommunicationServerParameters()
        {
            server.SendData(JsonConvert.SerializeObject(new CommunicationServerParameters()
            {
                PipelineId = PipelineId,
                IsMaster = true
            }));

        }

        public CommunicationResponse GetCommunicationResonse()
        {
            return JsonConvert.DeserializeObject<CommunicationResponse>(server.ReceiveData());
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
