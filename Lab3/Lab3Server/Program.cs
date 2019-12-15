using Lab3;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab3Server
{
    class Program
    {
        static IConnection conn { get; set; }
        static List<string> users = new List<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("запущено");
            conn = GetRabbitConnection();
            IModel server = GetRabbitChannel("server", "server", "server");
            var subscription = new Subscription(server, "server", false);
            while (true)
            {
                BasicDeliverEventArgs basicDeliveryEventArgs = subscription.Next();
                string messageContent = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);
                Message message = JsonConvert.DeserializeObject<Message>(messageContent);
                Console.WriteLine(message.Login + ": " + message.Text);
                if (message.Text == "Login")
                {
                    users.Add(message.Login);
                }
                else
                {
                    SendMessageToAll(message);
                }
                subscription.Ack(basicDeliveryEventArgs);
            }
        }
        static void SendMessageToAll(Message message)
        {
            foreach (string s in users)
            {
                IModel model = GetRabbitChannel(s, s, s);
                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                model.BasicPublish(s, s, null, messageBodyBytes);
            }
        }


        private static IModel GetRabbitChannel(string exchangeName, string queueName, string routingKey)
        {
            IModel model = conn.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            model.QueueDeclare(queueName, false, false, false, null);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }

        private static IConnection GetRabbitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            IConnection conn = factory.CreateConnection();
            return conn;
        }
    }
}
