using System;
using System.Text;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataTransport
{
    public class RabbitTransport
    {
        protected string userName;
        protected string passWord;
        protected string virtualHost;
        protected string hostName;

        public RabbitTransport()
        {
            //these get overridden by calling service
            userName = "userName";
            passWord = "passWord";
            virtualHost = "acme.dev";
            hostName = "rmq.company.com";
        }

        public RabbitTransport(string virtualHost) : this()
        {
            this.virtualHost = virtualHost;
        }

        public RabbitTransport(string virtualHost, string userName, string passWord) : this()
        {
            this.virtualHost = virtualHost;
            this.userName = userName;
            this.passWord = passWord;
        }

        public RabbitTransport(string hostName, string virtualHost, string userName, string passWord) : this()
        {
            this.hostName = hostName;
            this.virtualHost = virtualHost;
            this.userName = userName;
            this.passWord = passWord;
        }

        public void PublishMessage(string exchangeName, string routingKey, string messageBody)
        {
            PublishMessage(exchangeName, routingKey, messageBody, exchangeType: "direct", isDurable: true, isPersistent: true);
        }

        public void PublishMessage(string exchangeName, string routingKey, string messageBody, string exchangeType = "direct", bool isDurable = true, bool isPersistent = true, string ttl = null)
        {
            //right now we open/close connection for each message sent. Do we want to keep a global connection open?
            using (var connection = GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: isDurable);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = isPersistent; //mark messages as persistent if queue is durable (tells RabbitMQ to save the message to disk, but is not guaranteed)
                    if (!String.IsNullOrEmpty(ttl))
                        properties.Expiration = ttl; //message time to live (important for dead letter exchange)
                    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: Encoding.UTF8.GetBytes(messageBody));
                }
            }
        }

        public void PublishDeadLetterMessage(string exchangeName, string routingKey, string messageBody, string ttl)
        {
            //ensure ttl value is a number, because ttl is passed to rabbitmq as a string, but used as an integer (anys nulls or emtpy string will convert to default value)
            Int64 ittl;
            if (!Int64.TryParse(ttl, out ittl))
                ittl = 30000L; //30 seconds default
            PublishMessage(exchangeName, routingKey, messageBody, exchangeType: "direct", isDurable: true, isPersistent: true, ttl: ittl.ToString());
        }

        public void PublishDirectToQueue(string queueName, string messageBody)
        {
            using (var connection = GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    //if going direct to queue, ExchangeType=none, routingKey must equal queue name and Exchange Name not needed
                    //durable flag must match queue on server (queue durablity cannot be changed on server). durable means messages will not be lost if rabbitmq server stops.
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: Encoding.UTF8.GetBytes(messageBody));
                }
            }
        }

        public string GetSingleMessage(string queueName)
        {
            var msg = string.Empty;
            using (var connection = GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    BasicGetResult result = channel.BasicGet(queueName, false);
                    if (result != null)
                    {
                        msg = Encoding.UTF8.GetString(result.Body);
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }
            }
            return msg;
        }

        public List<string> Get(string vhost, string queue)
        {
            this.virtualHost = vhost ?? this.virtualHost;
            var list = new List<string>();
            using (var connection = GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new QueueingBasicConsumer(channel);
                    var queueDeclareResponse = channel.QueueDeclare(queue, true, false, false, null);
                    channel.BasicConsume(queue, false, consumer);
                    for (int i = 0; i < queueDeclareResponse.MessageCount; i++)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        list.Add(Encoding.UTF8.GetString(ea.Body));
                    }
                }
            }
            return list;
        }

        protected IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = AmqpTcpEndpoint.UseDefaultPort,
                UserName = userName,
                Password = passWord,
                VirtualHost = virtualHost,
                Protocol = Protocols.DefaultProtocol
            };
            return factory.CreateConnection(System.Environment.MachineName);
        }
    }
}
