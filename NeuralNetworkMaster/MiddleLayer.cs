using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster
{
    class MiddleLayer
    {
        private CommunicationLayer communicationLayer;

        public MiddleLayer(CommunicationLayer communicationLayer)
        {
            this.communicationLayer = communicationLayer;
        }

        public void SendInitialData(NeuralNetworkSlaveParameters neuralNetworkSlaveParameters ,String X,String y,String[] Theta)
        {

            string slaveJson = JsonConvert.SerializeObject(neuralNetworkSlaveParameters);
            communicationLayer.SendData(slaveJson);

            communicationLayer.SendDataSet(X);
            communicationLayer.SendDataSet(y);

            if(!neuralNetworkSlaveParameters.IsThetaNull)
            {
                for (int i = 0; i < Theta.Length; i++)
                    communicationLayer.SendDataSet(Theta[i]);
            }
        }

        public Matrix<double>[] BuildTheta(int hiddenLayerLength)
        {
            var thetaSize = int.Parse(communicationLayer.ReceiveData());
            var theta = JsonConvert.DeserializeObject<double[][,]>(communicationLayer.ReceiveData(thetaSize));
            var thetaMatrix = new Matrix<double>[hiddenLayerLength + 1];
            for (int i = 0; i < theta.Length; i++)
                thetaMatrix[i] = Matrix<double>.Build.Dense(theta[i].GetLength(0), theta[i].GetLength(1), (x, y) => (theta[i][x, y]));
            return thetaMatrix;
        }


    }
}
