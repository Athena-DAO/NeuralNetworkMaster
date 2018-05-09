using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster.Communication
{
    class CommunicationRabbitMq
    {
        public string QueueName { get; set; }
        private BlockingCollection<string> respQueue;

        public void Publish(string message)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {

                using (var channel = connection.CreateModel())
                {

                    channel.QueueDeclare(queue: QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);


                    

                    var body = Encoding.UTF8.GetBytes(message);


                    channel.BasicPublish(exchange: "",
                        routingKey: QueueName,
                        basicProperties: null,
                        body: body);



                  //  Console.WriteLine("[x] Sent {0}", message);

                }


            }
        
        }
        public string Consume()
        {

            respQueue = new BlockingCollection<string>();
            var factory = new ConnectionFactory() { HostName = "localhost" };


            using (var connection = factory.CreateConnection())
            {
                string message = "";
                using (var channel = connection.CreateModel())
                {

                    channel.QueueDeclare(queue: QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);


                    var consumer = new EventingBasicConsumer(channel);


                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        message = Encoding.UTF8.GetString(body);
                        respQueue.Add(message);
                    //    Console.WriteLine(" [x] Received {0}", message);
                    };

                    channel.BasicConsume(queue: QueueName,
                        autoAck: true,
                        consumer: consumer);

                    return respQueue.Take();

                }
            }
        }
    }
}
