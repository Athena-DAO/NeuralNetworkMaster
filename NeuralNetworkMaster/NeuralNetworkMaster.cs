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

        public String[] X_value;
        public String[] y_value;
        public int []TrainingSizes;

        public Matrix<double>[][] SlaveThetas;
        public Matrix<double>[] AverageTheta; 

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
            SlaveThetas = new Matrix<double>[NumberOfSlaves][];
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
                AverageTheta[i] = SlaveThetas[0][i].Clone();
                for (int j = 1; j < NumberOfSlaves; j++)
                    AverageTheta[i] = AverageTheta[i] + SlaveThetas[j][i];

                AverageTheta[i] = AverageTheta[i] / NumberOfSlaves;
                WriteCsv("Theta" + i +".csv", AverageTheta[i]);    
            }

            
        }

        public void Service(String ip, int port, int slaveNumber)
        {
            CommunicationLayer communicationLayer = new CommunicationLayer(ip, port);

            try
            {
                MiddleLayer middleLayer = new MiddleLayer(communicationLayer);

                NeuralNetworkSlaveParameters slave = new NeuralNetworkSlaveParameters
                {
                    InputLayerSize = InputLayerSize,
                    HiddenLayerSize = HiddenLayerSize,
                    HiddenLayerLength = HiddenLayerLength,
                    OutputLayerSize = OutputLayerSize,
                    TrainingSize = TrainingSizes[slaveNumber],
                    Lambda = Lambda,
                    Epoch = Epoch,
                    XDataSize = X_value[slaveNumber].Length,
                    YDataSize = y_value[slaveNumber].Length,
                    IsThetaNull = true,
                    ThetaSize = null
                };
                middleLayer.SendInitialData(slave, X_value[slaveNumber], y_value[slaveNumber], null);
                SlaveThetas[slaveNumber]=middleLayer.BuildTheta(HiddenLayerLength);
                
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception {0}", E);
            }
            finally
            {
                communicationLayer.Close();
            }

        }
    }

 
}