using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;


namespace Lab3
{
    public partial class Form1 : Form
    {
        //static List<User> users = new List<User>()
        //{
        //   new User(){Login="u1",Password="1",Flag=true },
        //   new User(){Login="u2",Password="2",Flag=true },
        //   new User(){Login="u3",Password="3",Flag=true }
        //};

        public string login { get; set; }
        IConnection conn { get; set; }
        private IModel GetRabbitChannel(string exchangeName, string queueName, string routingKey)
        {
            IModel model = conn.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            model.QueueDeclare(queueName, false, false, false, null);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }
        private IConnection GetRabbitConnection()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            IConnection conn = factory.CreateConnection();
            return conn;
        }

        public Form1()
        {
            InitializeComponent();
        }

        async Task UpdateWall()
        {
            await Task.Run(() =>
            {
                IModel model = GetRabbitChannel(login, login, login);
                var subscription = new Subscription(model, login, false);
                while (true)
                {
                    BasicDeliverEventArgs basicDeliveryEventArgs = subscription.Next();
                    string messageContent = Encoding.UTF8.GetString(basicDeliveryEventArgs.Body);
                    Message message = JsonConvert.DeserializeObject<Message>(messageContent);
                    if (message.Login == login)
                        message.Login = "Вы";

                    listBox1.Invoke((MethodInvoker)delegate 
                    {
                        listBox1.Items.Add($"{message.Login}: {message.Text}");
                    });
                   
                    subscription.Ack(basicDeliveryEventArgs);
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (String.IsNullOrEmpty(textBox1.Text))
                return;
            login = textBox1.Text;
            conn = GetRabbitConnection();
            SendMessage(new Message() { Login = login, Text = "Login" });
            UpdateWall();
            listBox1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            textBox2.Visible = true;
            label3.Visible = true;
            label3.Text = "Логин: " + login;
            button1.Visible = false;
            textBox1.Visible = false;
            label1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            textBox2.Visible = false;
            label3.Visible = false;
            textBox1.Text = "";
            button1.Visible = true;
            textBox1.Visible = true;
            label1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendMessage(new Message() { Login = login, Text = textBox2.Text });
            textBox2.Text = "";
        }
        void SendMessage(Message message)
        {
            IModel model = GetRabbitChannel("server", "server", "server");
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            model.BasicPublish("server", "server", null, messageBodyBytes);
        }
    }
}
