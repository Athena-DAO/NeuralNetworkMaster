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
            stream.BaseStream.Position = 0;
            stream.DiscardBufferedData();

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
            test(X_value[1]);
            test(y_value[0]);
            test(y_value[1]);

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

                SendInitialData(streamWriter, slaveNumber);

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


        private void SendInitialData(StreamWriter streamWriter, int slaveNumber)
        {
            streamWriter.WriteLine(InputLayerSize);
            streamWriter.WriteLine(HiddenLayerSize);
            streamWriter.WriteLine(HiddenLayerLength);
            streamWriter.WriteLine(OutputLayerSize);
            streamWriter.WriteLine(TrainingSizes[slaveNumber]);
            streamWriter.WriteLine(Lambda);
            streamWriter.WriteLine(Epoch);
            SendDataSet(streamWriter, X_value[slaveNumber]);
            SendDataSet(streamWriter, y_value[slaveNumber]);
        }


        private void SendDataSet(StreamWriter streamWriter,String dataSet)
        {
            int i = 0;
            int rem = dataSet.Length % 10240;
            streamWriter.WriteLine(dataSet.Length);
            while (i < (dataSet.Length - 10240))
            {
                streamWriter.Write(dataSet.Substring(i, 10240));
                i += 10240;
            }
            streamWriter.Write(dataSet.Substring(i, rem));
        }
       
    }

 
}