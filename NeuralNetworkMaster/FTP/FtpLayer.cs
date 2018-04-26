using System;
using System.Collections.Generic;
using System.Net;
using FluentFTP;
using System.Text;
using System.IO;

namespace NeuralNetworkMaster
{
    class FtpLayer
    {


        private FtpClient client;
        public FtpLayer(string ip , string userName, string password)
        {
            client = new FtpClient(ip);
            client.Credentials = new NetworkCredential(userName, password);

        }

        public void DownloadFile(String fileName)
        {
            client.Connect();
            client.DownloadFile(Directory.GetCurrentDirectory() + "//FileStore//" + fileName, "//" + fileName);
            client.Disconnect();
        }
    }
}