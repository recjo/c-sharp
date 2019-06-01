using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataTransport
{
    public class RabbitListener : RabbitTransport
    {
        //listener needs a persistent connection
        private IConnection connection;
        public IModel channel;

        public RabbitListener() : base()
        {
            if (connection == null)
                connection = GetConnection();
            if (channel == null)
                channel = connection.CreateModel();
        }

        public RabbitListener(string virtualHost, string userName, string passWord) : base()
        {
            base.virtualHost = virtualHost;
            base.userName = userName;
            base.passWord = passWord;
            if (connection == null)
                connection = GetConnection();
            if (channel == null)
                channel = connection.CreateModel();
        }

        public RabbitListener(string hostName, string virtualHost, string userName, string passWord) : base()
        {
            base.hostName = hostName;
            base.virtualHost = virtualHost;
            base.userName = userName;
            base.passWord = passWord;
            if (connection == null)
                connection = GetConnection();
            if (channel == null)
                channel = connection.CreateModel();
        }

        public string MessageListener(string queue, EventHandler<BasicDeliverEventArgs> eh)
        {
            try
            {
                //cannot dispose connection (i.e. "using" block) for each message received because the event handlers won't run before the connection is disposed
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += eh;
                channel.BasicConsume(queue: queue, noAck: false, consumer: consumer);
                return "Message listener setup completed.";
            }
            catch (Exception ex)
            {
                return String.Format("Error setting message listener: {0}", ex.Message);
            }
        }

        public string CloseConnection()
        {
            var msg = new StringBuilder();
            if (channel.IsOpen)
            {
                channel.Close();
                channel.Dispose();
                msg.Append("RabbitMQ Channel has been closed. ");
            }
            else
            {
                msg.Append("RabbitMQ Channel was already closed. ");
            }
            if (connection.IsOpen)
            {
                connection.Close();
                connection.Dispose();
                msg.Append("RabbitMQ connection has been closed.");
            }
            else
            {
                msg.Append("RabbitMQ connection was already closed.");
            }
            return msg.ToString();
        }
    }
}
