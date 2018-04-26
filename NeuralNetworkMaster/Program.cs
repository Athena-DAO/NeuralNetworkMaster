
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace NeuralNetworkMaster
{
    class Program
    {

        static void Main(string[] args)
        {
            string baseUrl = args[0];
            string pipelineId = args[1];

            
            /* 
             * GET parameters from api end point 
             * 
             * 
             */



            //CommunicationParameter communicationParameter = JsonConvert.DeserializeObject<CommunicationParameter>(args[1]);
            /*
            CommunicationModule communicationLayer = new CommunicationModule(7000);
            MiddleLayer middleLayer = new MiddleLayer(communicationLayer);
            NeuralNetworkMasterParameters masterParameters = middleLayer.GetInitialData();
            
            FtpLayer ftpLayer = new FtpLayer(masterParameters.DataSetUrl);

            ftpLayer.DownloadFile("X.csv");
            ftpLayer.DownloadFile("Y.csv");
            */
            NeuralNetworkMaster master = new NeuralNetworkMaster
            {
                PipelineId = pipelineId,
                NumberOfSlaves = 3,
                InputLayerSize = 400,
                HiddenLayerSize = 25,
                HiddenLayerLength = 1,
                OutputLayerSize = 10,
                Lambda = 3,
                Epoch = 50
                
            };

            master.Train();

        }
    }
}
