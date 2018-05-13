using NeuralNetworkMaster.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace NeuralNetworkMaster
{
    internal class WebHelper
    {
        public string Url { get; set; }
        public Credentials Credentials { get; set; }
        public string PipelineId { get; set; }
        private string Token;

        public void FetchJWT()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + @"/Account/Login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(Credentials);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string token;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                token = streamReader.ReadToEnd();
            }
            Token = JsonConvert.DeserializeObject<Token>(token).token;
        }

        public Pipeline GetPipeline()
        {
            string response = GetRequestWithJWT(@"/api/pipeline/" + PipelineId);
            return JsonConvert.DeserializeObject<Pipeline>(response);
        }

        public CompleteDataSet GetCompleteDataSet(string datasetId)
        {
            string response = GetRequestWithJWT(@"/api/completeDataSets/" + datasetId);
            return JsonConvert.DeserializeObject<CompleteDataSet>(response);
        }
        public void PostLog(string logMessage)
        {
            string json = JsonConvert.SerializeObject(new ApiEndPointLog()
            {
                PipelineId = PipelineId,
                Log = logMessage + "|\n"
            });
            PostRequestWithJWT(@"/api/Logging" ,json);
            
        }

        public void PostResult(string result)
        {
            string json = JsonConvert.SerializeObject(new ApiEndPointResult()
            {
                PipelineId = PipelineId,
                Result = result
            });
            PostRequestWithJWT(@"/api/pipeline/result", json);

        }

        private string GetRequestWithJWT(string endpoint)
        {
            string response;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + endpoint);
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);
            httpWebRequest.Accept = "application/json";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            return response;

        }

        private void PostRequestWithJWT(string endpoint, string message)
        {
            string response;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + endpoint);
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(message);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if(httpResponse.StatusCode !=HttpStatusCode.OK)
            {
                Console.WriteLine("Post Unsuccessful");
            }
        }
    }
}