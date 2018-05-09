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
        public CommunicationRabbitMq CommunicationRabbitMqM2s { get; set; }
        public CommunicationRabbitMq CommunicationRabbitMqS2M { get; set; }
        public bool P2P { get; set; }




        public NeuralNetworkMasterParameters GetInitialData()
        {
            return JsonConvert.DeserializeObject<NeuralNetworkMasterParameters>(CommunicationModule.ReceiveData());

        }

        public void SendInitialData(NeuralNetworkSlaveParameters neuralNetworkSlaveParameters ,String X,String y,String[] Theta)
        {
            string slaveJson = JsonConvert.SerializeObject(neuralNetworkSlaveParameters);
            if (P2P)
            {
                
                CommunicationModule.SendData(slaveJson);

                CommunicationModule.SendDataSet(X);
                CommunicationModule.SendDataSet(y);

                if (!neuralNetworkSlaveParameters.IsThetaNull)
                {
                    for (int i = 0; i < Theta.Length; i++)
                        CommunicationModule.SendDataSet(Theta[i]);
                }

            }else
            {
                CommunicationRabbitMqM2s.Publish(slaveJson);
                CommunicationRabbitMqM2s.Publish(X);
                CommunicationRabbitMqM2s.Publish(y);
                if (!neuralNetworkSlaveParameters.IsThetaNull)
                {
                    for (int i = 0; i < Theta.Length; i++)
                        CommunicationRabbitMqM2s.Publish(Theta[i]);
                }
            }
        }

        public Matrix<double>[] BuildTheta(int hiddenLayerLength)
        {
            string thetaJson;
            if (P2P)
            {
                var thetaSize = int.Parse(CommunicationModule.ReceiveData());
                thetaJson = CommunicationModule.ReceiveData(thetaSize);
                
            }
            else
            {
                thetaJson = CommunicationRabbitMqS2M.Consume(); 
            }

            var theta = JsonConvert.DeserializeObject<double[][,]>(thetaJson);
            var thetaMatrix = new Matrix<double>[hiddenLayerLength + 1];
            for (int i = 0; i < theta.Length; i++)
                thetaMatrix[i] = Matrix<double>.Build.Dense(theta[i].GetLength(0), theta[i].GetLength(1), (x, y) => (theta[i][x, y]));
            return thetaMatrix;
        }


    }
}
