using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoDownloader.Retrieval;
using DemoDownloader.RPC;
using DemoDownloader.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitTransfer;

namespace DemoDownloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<BlobStreamer>();
                    services.AddSingleton<BlobStorage>();

                    // Add the Rabbit Connection
                    services.AddSingleton(CreateRabbitConnection(hostContext.Configuration));
                    // Add The Rabbit RPC Server
                    services.AddHostedService<DemoDownloadServer>();

                    services.AddLogging(o =>
                    {
                        o.AddConsole();
                        o.AddDebug();
                    });
                });

        public static IConnection CreateRabbitConnection(IConfiguration configuration)
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
                return factory.CreateConnection();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create AMQP Connection, Please check " +
                    "the enviroment variables are set correctly", e);
            }
        }
    }
}
