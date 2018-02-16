using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace NeuralNetworkMaster
{
    class CommunicationLayer
    {

        public CommunicationLayer(String ip, int port, int slaveNumber)
        {
            TcpClient client = new TcpClient(ip, port);
            Stream stream = client.GetStream();
            
        }

        public void SendInitialData(Stream stream,NeuralNetworkMaster master, int slaveNumber)
        {
            NeuralNetworkSlaveParameters neuralNetworkCom = new NeuralNetworkSlaveParameters
            {
                InputLayerSize = master.InputLayerSize,
                HiddenLayerSize = master.HiddenLayerSize,
                HiddenLayerLength = master.HiddenLayerLength,
                OutputLayerSize = master.OutputLayerSize,
                TrainingSize = master.TrainingSizes[slaveNumber],
                Lambda = master.Lambda,
                Epoch = master.Epoch,
                XDataSize = master.X_value[slaveNumber].Length,
                YDataSize = master.y_value[slaveNumber].Length
            };

            string output = JsonConvert.SerializeObject(neuralNetworkCom);
            var bytes = Encoding.ASCII.GetBytes(output);
            stream.Write(bytes, 0, bytes.Length);
            ReceiveOk(stream);
            SendDataSet(stream, master.X_value[slaveNumber]);
            ReceiveOk(stream);
            SendDataSet(stream, master.y_value[slaveNumber]);
            ReceiveOk(stream);
        }


        private void SendDataSet(Stream stream, String dataSet)
        {
            int i = 0;
            int rem = dataSet.Length % 1024;
            while (i < (dataSet.Length - 1024))
            {
                byte[] msg = Encoding.ASCII.GetBytes(dataSet.Substring(i, 1024));
                stream.Write(msg, 0, 1024);
                i += 1024;
            }
            byte[] msg2 = Encoding.ASCII.GetBytes(dataSet.Substring(i, rem));
            stream.Write(msg2, 0, msg2.Length);
        }


        private static void ReceiveOk(Stream stream)
        {
            var recBytes = new byte[2];
            stream.Read(recBytes, 0, recBytes.Length);

            if (!(Encoding.ASCII.GetString(recBytes) == "Ok"))
            {
                throw new Exception("Ok Not received");
            }
        }

    }


    

}
