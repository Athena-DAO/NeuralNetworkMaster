using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NeuralNetworkMaster.Communication
{
    internal class CommunicationRabbitMq
    {
        private string QueueName;
        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;

        private List<string> Messages;
        private Object messageLock;

        public CommunicationRabbitMq(string queueName)
        {
            QueueName = queueName;
            messageLock = new object();
            factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
            channel.BasicQos(0, 1, false);
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: QueueName,
                basicProperties: null,
                body: body);

            //Console.WriteLine("[x] Sent {0}", message);
        }

        public void StartConsumer()
        {
            Messages = new List<string>();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                lock (messageLock)
                {
                    Messages.Add(message);
                    Monitor.Pulse(messageLock);

                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: QueueName,
                autoAck: false,
                consumer: consumer);
        }

        public string Consume()
        {
            string message;
            lock (messageLock)
            {
                while (Messages.Count == 0)
                {
                    Monitor.Pulse(messageLock);
                    Monitor.Wait(messageLock);
                }
                message = Messages[0];
                Messages.RemoveAt(0);
            }
            return message;
        }

        public void Close()
        {
            channel.QueueDelete(QueueName, false, false);
            channel.Close();
            connection.Close();
        }
    }
}