using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitTransfer;
using RabbitTransfer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoDownloader.RPC
{
    public class DownloadQueueConnection : IQueueConnection
    {
        public DownloadQueueConnection(IConfiguration configuration)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration.GetValue<string>("AMQP_HOST"),
                    UserName = configuration.GetValue<string>("AMQP_USER"),
                    VirtualHost = configuration.GetValue<string>("AMQP_VHOST"),
                    Password = configuration.GetValue<string>("AMQP_PASSWORD")
                };

                Connection = factory.CreateConnection();
                QueueName = RPCExchange.DC_DD.QUEUE;

            }
            catch (Exception e)
            {
                throw new Exception("Failed to create AMQP Connection, Please check " +
                    "the enviroment variables are set correctly", e);
            }
        }
        public IConnection Connection { get; set ; }
        public string QueueName { get ; set ; }
    }
}
