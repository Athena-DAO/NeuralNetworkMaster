using NeuralNetworkMaster.Model;
using System;
using System.IO;
using System.Net;

using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace NeuralNetworkMaster
{
    internal class Program
    {
        public static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            return builder.Build();
        }

        static void Main(string[] args)
        {
            string url = args[0];
            string pipelineId = args[1];
            IConfiguration Configuration = BuildConfiguration();

            Credentials credentials = new Credentials
            {
                UserName = "john.doe@gmail.com",
                Password = "P@ssw0rd"
            };
            WebHelper webHelper = new WebHelper()
            {
                Credentials = credentials,
                PipelineId = pipelineId,
                Url = url
            };

            webHelper.FetchJWT();
            Pipeline pipeline = webHelper.GetPipeline();


            Dictionary<string, string> masterParameters = BuildMasterParameters(pipeline.parameters);
            string ftpUrl = "ftp" + ":" + url.Split(':')[1];
            FtpLayer ftpLayer = new FtpLayer(ftpUrl, "kishan", "athena_123");
           
            ftpLayer.DownloadFile(masterParameters["XFileName"]);
            ftpLayer.DownloadFile(masterParameters["YFileName"]);



            NeuralNetworkMaster master = new NeuralNetworkMaster
            {
                PipelineId = pipelineId,
                NumberOfSlaves = pipeline.numberOfContainers - 1,
                InputLayerSize = int.Parse(masterParameters["InputLayerSize"]),
                HiddenLayerSize = int.Parse(masterParameters["HiddenLayerSize"]),
                HiddenLayerLength = int.Parse(masterParameters["HiddenLayerLength"]),
                OutputLayerSize = int.Parse(masterParameters["OutputLayerSize"]),
                Lambda = int.Parse(masterParameters["Lambda"]),
                Epoch = int.Parse(masterParameters["Epoch"]),
                XFileName = masterParameters["XFileName"],
                YFileName = masterParameters["YFileName"],
                Configuration = Configuration,
                WebHelper = webHelper
            };
            master.Train();
        }

        private static Dictionary<string,string> BuildMasterParameters(List<PipelineParameters> parameters)
        {
            Dictionary<string, string> masterParameters = new Dictionary<string, string>();

            foreach(var parameter in parameters)
            {
                masterParameters.Add(parameter.parameterName, parameter.value);
            }
            return masterParameters;
        }
    }
}