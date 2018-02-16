using Newtonsoft.Json;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NeuralNetworkMaster
{
    internal class NeuralNetworkMaster
    {
        public int NumberOfSlaves { get; set; }
        public int InputLayerSize { get; set; }
        public int HiddenLayerSize { get; set; }
        public int HiddenLayerLength { get; set; }
        public int OutputLayerSize { get; set; }
        public double Lambda { get; set; }
        public int Epoch { get; set; }
        //private TcpListener tcpListener;
        public String[] X_value;
        public String[] y_value;
        public int []TrainingSizes;

        public Matrix<double>[][] Thetas;
        public Matrix<double>[] AverageTheta; 
        private Thread[] threads;
        public NeuralNetworkMaster()
        {
            
        }

        public String[] SplitDataSet(string path, int numberOfSlaves)
        {
            StreamReader stream = new StreamReader(path);
            string[] dataSet = new string[numberOfSlaves];
            int lines = 0;
            while (!stream.EndOfStream)
            {
                stream.ReadLine();
                lines++;
            }
            stream.Close();
            stream = new StreamReader(path);
            int numberOfLines = (int)lines / numberOfSlaves;
            int remainderLines = lines % numberOfSlaves;

            for (int i = 0; i < numberOfSlaves - 1; i++)
            {
                StringBuilder stringBuilder = new StringBuilder(numberOfLines);
                for (int j = 0; j < numberOfLines-1; j++)
                {
                    stringBuilder.AppendLine(stream.ReadLine());
                }
                stringBuilder.Append(stream.ReadLine());
                dataSet[i] = stringBuilder.ToString();
                TrainingSizes[i] = numberOfLines;


            }
            StringBuilder stringBuilderLast = new StringBuilder(remainderLines*1000);
            while (!stream.EndOfStream)
            {
                numberOfLines = numberOfLines + remainderLines;
                for (int j = 0; j < numberOfLines - 1; j++)
                {
                    stringBuilderLast.AppendLine(stream.ReadLine());
                }
                stringBuilderLast.Append(stream.ReadLine());
            }

            dataSet[numberOfSlaves - 1] = stringBuilderLast.ToString();
            TrainingSizes[NumberOfSlaves - 1] = numberOfLines;
            return dataSet;
        }
        public static void WriteCsv(string path, Matrix<double> matrix)
        {
            StreamWriter stream = new StreamWriter(path);

            for (int i = 0; i < matrix.RowCount; i++)
            {
                var result = string.Join(",", matrix.Row(i).ToArray());
                stream.WriteLine(result);
            }
            stream.Close();
        }

        public void Master()
        {
            Thetas = new Matrix<double>[NumberOfSlaves][];
            AverageTheta = new Matrix<double>[HiddenLayerLength + 1];
            TrainingSizes = new int[NumberOfSlaves];
            X_value = SplitDataSet("X_value.csv", NumberOfSlaves);
            y_value = SplitDataSet("Y_value.csv", NumberOfSlaves);

            var thread = new Thread[NumberOfSlaves];
            for (int i = 0; i < NumberOfSlaves; i++)
            {
                var slaveNumber = i;
                thread[i] = new Thread(() => Service("127.0.0.1", 6000+slaveNumber, slaveNumber));
                thread[i].Start();
            }

            foreach(var t in thread)
            {
                t.Join();
            }
            for (int i = 0; i < (HiddenLayerLength+1); i++)
            {
                AverageTheta[i] = Thetas[0][i].Clone();
                for (int j = 1; j < NumberOfSlaves; j++)
                    AverageTheta[i] = AverageTheta[i] + Thetas[j][i];

                AverageTheta[i] = AverageTheta[i] / NumberOfSlaves;
                WriteCsv("Theata" + i +".csv", AverageTheta[i]);    
            }

            
        }

        public void Service(String ip, int port, int slaveNumber)
        {
            TcpClient client = new TcpClient(ip, port);

            try
            {
                Stream stream = client.GetStream();
                SendInitialData(stream, slaveNumber);
                var ThetaSize = int.Parse(GetJSONData(stream));
                var s = GetDataSet(stream, ThetaSize);
                var Theta = JsonConvert.DeserializeObject<double[][,]>(s);
                Thetas[slaveNumber] = new Matrix<double>[HiddenLayerLength + 1];
                for (int i = 0; i < Theta.Length; i++)
                    Thetas[slaveNumber][i] = Matrix<double>.Build.Dense(Theta[i].GetLength(0), Theta[i].GetLength(1), (x, y) => (Theta[i][x, y]));
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception {0}", E);
            }
            finally
            {
                client.Close();
            }



        }

        public string GetDataSet(Stream stream,int filesize)
        {
            var buffer = new byte[1024];
            var lines = new List<double[]>();
            int receivedSize = 0;
            int bytesReceived;
            StringBuilder stringBuilder = new StringBuilder(filesize);

            while (receivedSize < (filesize) && (bytesReceived = stream.Read(buffer, 0, 1024)) != 0)
            {
                String msg = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                stringBuilder.Append(msg, 0, bytesReceived);
                receivedSize += bytesReceived;
                Console.WriteLine(receivedSize);
            }
            SendOk(stream);
            return stringBuilder.ToString();
        }






        private void SendInitialData(Stream stream, int slaveNumber)
        {
            NeuralNetworkSlaveParameters neuralNetworkCom = new NeuralNetworkSlaveParameters
            {
                InputLayerSize = InputLayerSize,
                HiddenLayerSize = HiddenLayerSize,
                HiddenLayerLength = HiddenLayerLength,
                OutputLayerSize = OutputLayerSize,
                TrainingSize = TrainingSizes[slaveNumber],
                Lambda = Lambda,
                Epoch = Epoch,
                XDataSize = X_value[slaveNumber].Length,
                YDataSize = y_value[slaveNumber].Length
            };

            string output = JsonConvert.SerializeObject(neuralNetworkCom);
            var bytes = Encoding.ASCII.GetBytes(output);
            stream.Write(bytes, 0, bytes.Length);
            ReceiveOk(stream);
            SendDataSet(stream ,X_value[slaveNumber]);
            ReceiveOk(stream);
            SendDataSet(stream, y_value[slaveNumber]);
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

        public String GetJSONData(Stream stream)
        {
            var bytes = new byte[1024];
            int received = stream.Read(bytes, 0, 1024);
            SendOk(stream);
            return Encoding.ASCII.GetString(bytes, 0, received);
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

        private void SendOk(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("Ok");
            stream.Write(bytes, 0, bytes.Length);
        }
    }

 
}