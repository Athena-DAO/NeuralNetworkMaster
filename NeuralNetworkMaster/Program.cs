using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace NeuralNetworkMaster
{
    class Program
    {

        static void Main(string[] args)
        {
            CommunicationParameter communicationParameter = JsonConvert.DeserializeObject<CommunicationParameter>(args[1]);
            CommunicationLayer communicationLayer = new CommunicationLayer(communicationParameter.Port);
            MiddleLayer middleLayer = new MiddleLayer(communicationLayer);
            NeuralNetworkMasterParameters masterParameters = middleLayer.GetInitialData();
            FtpLayer ftpLayer = new FtpLayer(masterParameters.DataSetUrl);
            ftpLayer.DownloadFile("X.csv");
            ftpLayer.DownloadFile("Y.csv");

            NeuralNetworkMaster master = new NeuralNetworkMaster
            {
                NumberOfSlaves = communicationParameter.NumberOfSlaves,
                InputLayerSize = masterParameters.InputLayerSize,
                HiddenLayerSize = masterParameters.HiddenLayerSize,
                HiddenLayerLength = masterParameters.HiddenLayerLength,
                OutputLayerSize = masterParameters.OutputLayerSize,
                Lambda = masterParameters.Lambda,
                Epoch = masterParameters.Epoch
                
            };

            master.Master(communicationParameter.SlaveLocations);

        }
    }
}
