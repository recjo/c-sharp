using System;
using System.Configuration;
using DataTransport;

namespace .DataIngestor
{
    public class Publish
    {
        //all non-relevant code removed for brevity
        private RabbitTransport _rb;

        public Publish()
        {
            var host = ConfigurationManager.AppSettings["RabbitMqHostName"];
            var vhost = ConfigurationManager.AppSettings["RabbitMqVirtualHost"];
            var uname = ConfigurationManager.AppSettings["RabbitMqIngestorUname"];
            var pwd = ConfigurationManager.AppSettings["RabbitMqIngestorPwd"];
            _rb = new RabbitTransport(host, vhost, uname, pwd);
        }

        public void PublishMessage(string msg, string rabbitMqExchangeName, string rabbitMqRoutingKey)
        {
            _rb.PublishMessage(rabbitMqExchangeName, rabbitMqRoutingKey, msg);
        }
    }
}
