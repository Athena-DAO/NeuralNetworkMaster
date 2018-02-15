using Newtonsoft.Json;
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
        private String[] X_value;

        private String[] y_value;
        private int []TrainingSizes;

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

        public void test(String s)
        {
            var lines = new List<double[]>();
            var Data = s.Split("\n");
            Console.WriteLine("DataLength =" + Data.Length);
            for (int i = 0; i < Data.Length; i++)
            {
                string[] line = Data[i].Split(',');
                var lineValues = new double[line.Length];
                lineValues = line.Select(e => Convert.ToDouble(e)).ToArray();
                lines.Add(lineValues);
            }

            var data = lines.ToArray();

        }



        public void Master()
        {
            TrainingSizes = new int[NumberOfSlaves];
            X_value = SplitDataSet("X_value.csv", NumberOfSlaves);
            y_value = SplitDataSet("Y_value.csv", NumberOfSlaves);


            test(X_value[0]);
            //test(X_value[1]);
            test(y_value[0]);
            //test(y_value[1]);

            var thread = new Thread[NumberOfSlaves];
            for (int i = 0; i < NumberOfSlaves; i++)
            {
                var slaveNumber = i;
                thread[i] = new Thread(() => Service("127.0.0.1", 6000+slaveNumber, slaveNumber));
                thread[i].Start();
            }
        }

        public void Service(String ip, int port, int slaveNumber)
        {
            TcpClient client = new TcpClient(ip, port);

            try
            {
                Stream stream = client.GetStream();

                StreamReader streamReader = new StreamReader(stream);
                StreamWriter streamWriter = new StreamWriter(stream);

                streamWriter.AutoFlush = true;
                SendInitialData(streamWriter,stream, slaveNumber);

                Console.WriteLine(streamReader.ReadLine() + " Slave " + slaveNumber);
                var x = Console.ReadLine();
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


        private void SendInitialData(StreamWriter streamWriter,Stream stream, int slaveNumber)
        {
            NeuralNetworkCom neuralNetworkCom = new NeuralNetworkCom
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
            SendDataSet(streamWriter,stream ,X_value[slaveNumber]);
            ReceiveOk(stream);
            SendDataSet(streamWriter,stream, y_value[slaveNumber]);
            ReceiveOk(stream);
        }


        private void SendDataSet(StreamWriter streamWriter, Stream stream, String dataSet)
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