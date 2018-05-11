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
            string baseUrl = args[0];
            string pipelineId = args[1];
            IConfiguration Configuration = BuildConfiguration();

            Credentials credentials = new Credentials
            {
                UserName = "john.doe@gmail.com",
                Password = "P@ssw0rd"
            };

            string token = GetJWT( baseUrl + @"/Account/Login" , credentials);
            Pipeline pipeline = GetPipeline(baseUrl,pipelineId, token);


            Dictionary<string, string> masterParameters = BuildMasterParameters(pipeline.parameters);
            FtpLayer ftpLayer = new FtpLayer(masterParameters["DataSetUrl"], "kishan", "lalit_123");
           
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
                Epoch = 50
            };
            
            master.Train();
        }

        private static string GetJWT(string url , Credentials credentials)
        {
            string jwt;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(credentials);


                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                jwt = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<Token>(jwt).token;
        }

        private static Pipeline GetPipeline(string baseUrl ,string pipelineId , string token)
        {
            string json;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl + @"/api/pipeline/" + pipelineId);
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);
            httpWebRequest.Accept = "application/json";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                json = streamReader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<Pipeline>(json);
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