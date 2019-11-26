using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemoDownloader
{
    public class Worker : BackgroundService
    {

        private readonly ILogger<Worker> logger;
        private readonly IBlobStreamer blobStreamer;

        public Worker(ILogger<Worker> logger, IBlobStreamer blobStreamer)
        {
            this.logger = logger;
            this.blobStreamer = blobStreamer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                string fileUrl = "https://demos-europe-west2.faceit-cdn.net/csgo/418fc933-9f72-418e-815f-566c86f125e0.dem.gz";
                await blobStreamer.StreamToBlobAsync(fileUrl);
                await Task.Delay(100000, stoppingToken);
            }
        }
    }
}
