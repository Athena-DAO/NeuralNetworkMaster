using MathNet.Numerics.LinearAlgebra;
using NeuralNetworkMaster.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster
{
    class MiddleLayer
    {
        public CommunicationModule CommunicationModule { get; set; }


        public void SendInitialData(NeuralNetworkSlaveParameters neuralNetworkSlaveParameters ,String X,String y,String[] Theta)
        {
            string slaveJson = JsonConvert.SerializeObject(neuralNetworkSlaveParameters);

            CommunicationModule.SendData(slaveJson, false);
            CommunicationModule.SendData(X, true);
            CommunicationModule.SendData(y, true);
            if (!neuralNetworkSlaveParameters.IsThetaNull)
            {
                for (int i = 0; i < Theta.Length; i++)
                    CommunicationModule.SendData(Theta[i],true);
            }
        }
        public string ReceiveTheta(int size)
        {
            return CommunicationModule.ReceiveData(size);
        }

        public Matrix<double>[] BuildTheta(string thetaJson, int hiddenLayerLength)
        {
            var theta = JsonConvert.DeserializeObject<double[][,]>(thetaJson);
            var thetaMatrix = new Matrix<double>[hiddenLayerLength + 1];
            for (int i = 0; i < theta.Length; i++)
                thetaMatrix[i] = Matrix<double>.Build.Dense(theta[i].GetLength(0), theta[i].GetLength(1), (x, y) => (theta[i][x, y]));
            return thetaMatrix;
        }


    }
}
