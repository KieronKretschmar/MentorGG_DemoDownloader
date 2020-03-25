using System;
using DemoDownloader.Retrieval;
using DemoDownloader.RPC;
using DemoDownloader.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitCommunicationLib.Queues;

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
                    services.AddHostedService(sp =>
                        {
                            return UrlConsumerFactory(sp, hostContext.Configuration);
                        }
                    );

                    services.AddLogging(options =>
                    { 
                        options.AddConsole(o =>
                        {
                            o.TimestampFormat = "[yyyy-MM-dd HH:mm:ss zzz] ";
                        });
                    });
                });

        /// <summary>
        /// Return a UrlConsumer.
        /// </summary>
        private static UrlConsumer UrlConsumerFactory(IServiceProvider sp, IConfiguration config)
        {
            string AMQP_URI = GetRequiredEnvironmentVariable<string>(config, "AMQP_URI");
            string AMQP_DOWNLOAD_URL_QUEUE = GetRequiredEnvironmentVariable<string>(config, "AMQP_DOWNLOAD_URL_QUEUE");
            string AMQP_DEMO_URL_QUEUE = GetRequiredEnvironmentVariable<string>(config, "AMQP_DEMO_URL_QUEUE");
            ushort AMQP_PREFETCH_COUNT = GetRequiredEnvironmentVariable<ushort>(config, "AMQP_PREFETCH_COUNT");

            var connections = new RPCQueueConnections(
                AMQP_URI,
                AMQP_DOWNLOAD_URL_QUEUE,
                AMQP_DEMO_URL_QUEUE
            );

            return new UrlConsumer(
                logger: sp.GetRequiredService<ILogger<UrlConsumer>>(),
                blobStreamer: sp.GetRequiredService<BlobStreamer>(),
                queueConnections: connections,
                prefetchCount: AMQP_PREFETCH_COUNT);
        }

        /// <summary>
        /// Attempt to retrieve an Environment Variable
        /// Throws ArgumentNullException is not found.
        /// </summary>
        /// <typeparam name="T">Type to retreive</typeparam>
        private static T GetRequiredEnvironmentVariable<T>(IConfiguration config, string key)
        {
            T value = config.GetValue<T>(key);
            if (value == null)
            {
                throw new ArgumentNullException(
                    $"{key} is missing, Configure the `{key}` environment variable.");
            }
            else
            {
                return value;
            }
        }

    }
}
