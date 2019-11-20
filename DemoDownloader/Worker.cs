using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DemoDownloader
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                string fileUrl = "https://demos-europe-west2.faceit-cdn.net/csgo/418fc933-9f72-418e-815f-566c86f125e0.dem.gz";
                string outputFilePath;
                bool result = Download.AttemptDownload(fileUrl, out outputFilePath);
                await Task.Delay(100000, stoppingToken);
            }
        }
    }
}
