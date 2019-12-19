using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoDownloader.Retrieval;
using DemoDownloader.RPC;
using DemoDownloader.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitTransfer.Queues;

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

                    // Add The Rabbit RPC Server
                    services.AddHostedService(
                        sp =>
                        {
                            return new UrlConsumer(
                                logger: sp.GetRequiredService<ILogger<UrlConsumer>>(),
                                blobStreamer: sp.GetRequiredService<BlobStreamer>(),
                                queueConnections: new RPCQueueConnections(
                                    hostContext.Configuration.GetValue<string>("AMQP_URI"),
                                    hostContext.Configuration.GetValue<string>("AMQP_DOWNLOAD_URL_QUEUE"),
                                    hostContext.Configuration.GetValue<string>("AMQP_DEMO_URL_QUEUE")
                                    )
                                );
                        }
                    );

                    services.AddLogging(o =>
                    {
                        o.AddConsole();
                        o.AddDebug();
                    });
                });

    }
}
