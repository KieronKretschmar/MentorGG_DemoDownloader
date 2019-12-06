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

        private readonly ILogger<Worker> _logger;
        private readonly BlobStreamer _blobStreamer;

        public Worker(ILogger<Worker> logger, BlobStreamer blobStreamer)
        {
            _logger = logger;
            _blobStreamer = blobStreamer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                string[] valid_test_urls = {
                    "http://replay191.valve.net/730/003380559004810871024_1371365513.dem.bz2",
                    "https://demos-asia-southeast1.faceit-cdn.net/csgo/76d083e1-9808-48e4-aaf0-9d1d49343b28.dem.gz",
                    "http://replay186.valve.net/730/003380681898857595197_1318256891.dem.bz2",
                    "https://demos-europe-west2.faceit-cdn.net/csgo/5bb0cbe3-4045-45dd-a7c1-064422e52762.dem.gz",
                    "https://demos-europe-west2.faceit-cdn.net/csgo/2af4b0e3-084e-4dd9-90e0-72b4f342ccbb.dem.gz",
                    "https://demos-europe-west2.faceit-cdn.net/csgo/418fc933-9f72-418e-815f-566c86f125e0.dem.gz",
                };

                for (int i = 0; i < valid_test_urls.Length; i++)
                {
                    try
                    {
                        await _blobStreamer.StreamToBlobAsync(valid_test_urls[i]);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }

                await Task.Delay(100000, stoppingToken);
            }
        }
    }
}
