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

        public Matrix<double>[][] ThetaSlaves;
        public string[] Theta;
        public Matrix<double>[] AverageTheta;

        public NeuralNetworkMaster()
        {
            
        }

        public String[] SplitDataSet(string path, int numberOfSlaves)
        {
            FileAccess fileAccess = new FileAccess(path);
            string[] dataSet = new string[numberOfSlaves];

            int numberOfLines = (int)fileAccess.Lines / numberOfSlaves;
            int numberOfLinesLastSlave = numberOfLines + fileAccess.Lines % numberOfSlaves;

            fileAccess.OpenStream();
            for (int i = 0; i < numberOfSlaves-1; i++)
            {
                dataSet[i] = fileAccess.ReadNextLines(numberOfLines);
                TrainingSizes[i] = numberOfLines;
            }

            dataSet[numberOfSlaves - 1] = fileAccess.ReadNextLines(numberOfLinesLastSlave) ;
            TrainingSizes[NumberOfSlaves - 1] = numberOfLinesLastSlave;
            fileAccess.CloseStream();
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
            ThetaSlaves = new Matrix<double>[NumberOfSlaves][];
            AverageTheta = new Matrix<double>[HiddenLayerLength + 1];
            TrainingSizes = new int[NumberOfSlaves];
            X_value = SplitDataSet("X_value.csv", NumberOfSlaves);
            y_value = SplitDataSet("Y_value.csv", NumberOfSlaves);

            var threads = new Thread[NumberOfSlaves];
            for (int i = 0; i < NumberOfSlaves; i++)
            {
                var slaveNumber = i;
                threads[i] = new Thread(() => Service("127.0.0.1", 6000+slaveNumber, slaveNumber));
                threads[i].Start();
            }

            foreach(var thread in threads)
            {
                thread.Join();
            }

            ComputeThetaAvereage();

            //Retrain();
            
        }

        /*
        public void Retrain()
        {
            FileAccess fileAccessTheta0 = new FileAccess("Theta0.csv");
            FileAccess fileAccessTheta1 = new FileAccess("Theta1.csv");
            Theta = new string[HiddenLayerLength + 1];

            Theta[0] = fileAccessTheta0.ReadFile();
            Theta[1] = fileAccessTheta1.ReadFile();

            var t = new Thread(() => Service("127.0.0.1", 6000 + NumberOfSlaves, 0));
            t.Start();
            t.Join();
        }
        */
        public void Service(String ip, int port, int slaveNumber)
        {
            CommunicationLayer communicationLayer = new CommunicationLayer(ip, port);

            try
            {
                MiddleLayer middleLayer = new MiddleLayer(communicationLayer);
                var slaveParameters= BuildSlaveParameters(slaveNumber);
                middleLayer.SendInitialData(slaveParameters, X_value[slaveNumber], y_value[slaveNumber], Theta);
                ThetaSlaves[slaveNumber]=middleLayer.BuildTheta(HiddenLayerLength);
                
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
        
        private NeuralNetworkSlaveParameters BuildSlaveParameters(int slaveNumber)
        {

                        
            bool isThetaNull = (Theta == null ? true : false);
            int[] thetaSize = null;

            if (!isThetaNull)
            {
                thetaSize = new int[HiddenLayerLength + 1];

                for (int i = 0; i < (HiddenLayerLength + 1); i++)
                {
                    thetaSize[i] = Theta[i].Length;
                }
            }

            return new NeuralNetworkSlaveParameters
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
                IsThetaNull = isThetaNull,
                ThetaSize = thetaSize
            };
        }

        private void ComputeThetaAvereage()
        {
            for (int i = 0; i < (HiddenLayerLength + 1); i++)
            {
                AverageTheta[i] = ThetaSlaves[0][i].Clone();
                for (int j = 1; j < NumberOfSlaves; j++)
                    AverageTheta[i] = AverageTheta[i] + ThetaSlaves[j][i];

                AverageTheta[i] = AverageTheta[i] / NumberOfSlaves;
                WriteCsv("Theta" + i + ".csv", AverageTheta[i]);
            }
        }
    }

 
}