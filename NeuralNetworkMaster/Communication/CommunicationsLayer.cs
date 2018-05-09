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
    class CommunicationsLayer
    {
        public CommunicationModule server { get; set; }
        public string PipelineId { get; set; }

        public IConfiguration Configuration { get; set; }
        public CommunicationsLayer()
        {
            BuildConfiguration();
            server = new CommunicationModule($"{Configuration["Ip-CommunicationServer"]}", int.Parse($"{Configuration["Port-CommunicationServer"]}"));
        }

        public void BuildConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
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
