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
        public FtpLayer()
        {
            client = new FtpClient("127.0.0.1");

        }


        public void DownloadFile(String fileName)
        {

            client.Connect();
            client.DownloadFile(Directory.GetCurrentDirectory() + "//FileStore//" + fileName, "//" + fileName);
            client.Disconnect();
        }
    }
}