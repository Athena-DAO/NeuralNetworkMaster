using NeuralNetworkMaster.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NeuralNetworkMaster
{
    class WebHelper
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
            string json;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + @"/api/pipeline/" + PipelineId);
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + Token);
            httpWebRequest.Accept = "application/json";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                json = streamReader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<Pipeline>(json);
        }

        public void SendLog(string jsonLog)
        {

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + @"/api/pipeline/" + PipelineId);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonLog);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
    }
}
