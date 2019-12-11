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
using RabbitTransfer.Interfaces;

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

                    // Add the Rabbit Connection Factory
                    // A new instance will be created for each use of `IQueueConnection`
                    services.AddTransient<IQueueConnection>(
                        serviceProvider => { return new DownloadQueueConnection(hostContext.Configuration); }
                    );


                    // Add The Rabbit RPC Server
                    services.AddHostedService<DemoDownloadServer>();

                    services.AddLogging(o =>
                    {
                        o.AddConsole();
                        o.AddDebug();
                    });
                });

    }
}
