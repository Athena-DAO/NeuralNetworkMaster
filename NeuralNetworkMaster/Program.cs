using NeuralNetworkMaster.Model;
using System;
using System.IO;
using System.Net;

using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;

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
                UserName = $"{Configuration["username-athena"]}",
                Password = $"{Configuration["password-athena"]}"
            };

            WebHelper webHelper = new WebHelper()
            {
                Credentials = credentials,
                PipelineId = pipelineId,
                Url = url
            };

            webHelper.FetchJWT();
            Pipeline pipeline = webHelper.GetPipeline();
            CompleteDataSet completeDataSet = webHelper.GetCompleteDataSet(pipeline.dataSetId);

            Dictionary<string, string> masterParameters = BuildMasterParameters(pipeline.parameters);

            string ftpUrl = "ftp" + ":" + url.Split(':')[1];
            FtpLayer ftpLayer = new FtpLayer(ftpUrl, $"{Configuration["username-ftp"]}", $"{Configuration["password-ftp"]}");
           
            ftpLayer.DownloadFile(completeDataSet.xComponentId);
            ftpLayer.DownloadFile(completeDataSet.yComponentId);



            NeuralNetworkMaster master = new NeuralNetworkMaster
            {
                PipelineId = pipelineId,
                NumberOfSlaves = pipeline.numberOfContainers - 1,
                InputLayerSize = int.Parse(masterParameters["InputLayerSize"]),
                HiddenLayerSize = int.Parse(masterParameters["HiddenLayerSize"]),
                HiddenLayerLength = int.Parse(masterParameters["HiddenLayerLength"]),
                OutputLayerSize = int.Parse(masterParameters["OutputLayerSize"]),
                Lambda = int.Parse(masterParameters["Lambda"]),
                Epoch = 50,//int.Parse(masterParameters["Epoch"]),
                XFileName = completeDataSet.xComponentId,
                YFileName = completeDataSet.yComponentId,
                Configuration = Configuration,
                WebHelper = webHelper
            };
            master.Train();

            StringBuilder files = new StringBuilder();
            int hiddenLayerLength = int.Parse(masterParameters["HiddenLayerLength"]);
            for (int i = 0; i < (hiddenLayerLength + 1); i++)
            {

                ftpLayer.UploadFile(pipelineId + "_Theta_" + i);

                files.Append(pipelineId + "_Theta_" + i + ".csv,");
            }
            files.Remove(files.Length - 1, 1);

            webHelper.PostResult(files.ToString());
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