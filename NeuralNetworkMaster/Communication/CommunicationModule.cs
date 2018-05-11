using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster.Communication
{
    class CommunicationModule
    {
        public CommunicationTcp CommunicationTcp { get; set; }
        public CommunicationRabbitMq CommunicationRabbitMqM2S { get; set; }
        public CommunicationRabbitMq CommunicationRabbitMqS2M { get; set; }
        public bool P2P { get; set; }

        public void SendData(string message,bool sendSize)
        {
            if (P2P)
            {
                if(sendSize)
                {
                    CommunicationTcp.SendData(message.Length.ToString());
                    CommunicationTcp.SendData(message);
                }
                else
                {
                    CommunicationTcp.SendData(message);
                }
            }
            else
                CommunicationRabbitMqM2S.Publish(message);
        }

        public string ReceiveData(int size=0)
        {
            if (P2P)
            {
                if (size == 0)
                {
                    return CommunicationTcp.ReceiveData();
                }
                else if(size <0)
                {
                    return CommunicationTcp.ReceiveData(int.Parse(CommunicationTcp.ReceiveData()));
                }
                else
                {
                    return CommunicationTcp.ReceiveData(size);
                }
            }
            else
                return CommunicationRabbitMqS2M.Consume();

        }

    }
}
