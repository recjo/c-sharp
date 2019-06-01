using System;
using System.Text;
using System.Configuration;
using System.ServiceProcess;
using RabbitMQ.Client.Events;
using .DataTransport;

namespace DataPusher
{
    public partial class Service1 : ServiceBase
    {
        private RabbitListener _rbListenQ2;
   
        public Service1()
        {
            InitializeComponent();
			var host = ConfigurationManager.AppSettings["RabbitMqHostName"];
			var vhost = ConfigurationManager.AppSettings["RabbitMqVirtualHost"];
			var uname = ConfigurationManager.AppSettings["RabbitMqPusherConsumeUname"];
			var pwd = ConfigurationManager.AppSettings["RabbitMqPusherConsumePwd"];
            _rbListenQ2 = new RabbitListener(host, vhost, uname, pwd);
        }

        protected override void OnStart(string[] args)
        {
            ListenForMessages();
        }

        protected override void OnStop()
        {
            var result = _rbListenQ2.CloseConnection();

        }

        public void ListenForMessages()
        {
            var queue2Name = ConfigurationManager.AppSettings["RabbitMqQueueNameOutbound"];
			//pass name of method to handle rabbit callbacks
            var result = _rbListenQ2.MessageListener(queue2Name, ProcessMessage);
        }

        //callback from RabbitMQ's consumer receive event handler (every time message is received)
        public void ProcessMessage(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var msg = Encoding.UTF8.GetString(ea.Body);

                // *******************************************
                // code to process message removed for brevity
                // *******************************************

                //send ack message to remove from queue
                _rbListenQ2.channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                var errmsg = String.Format("Data Pusher root exception occurred before _taskService.StartTask(), Err: {0}", ex.Message);
                //_loggingService.LogException(LogEntryTypeCode.Error, ErrorTypeCode.PushError, _processName, errmsg, 0);
                //this is a task with bad payload that bombed BEFORE _taskService.StartTask(). Saved to db (if possible)
                //send ack to Rabbit to remove from queue 1, or it will stay perpetually unacked
                _rbListenQ2.channel.BasicAck(ea.DeliveryTag, false);
            }
        }
    }
}
