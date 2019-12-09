using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RabbitTransfer;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace DemoDownloader.RPC
{
    public class DemoDownloaderRPCServer : AbstractRPCServer, IHostedService
    {
        private readonly ILogger<DemoDownloaderRPCServer> _logger;
        private readonly BlobStreamer _blobStreamer;

        public override string QUEUE_NAME => RPCExchange.DC_DD.QUEUE;

        public DemoDownloaderRPCServer(
            ILogger<DemoDownloaderRPCServer> logger,
            BlobStreamer blobStreamer) : base()
        {
            this._logger = logger;
            this._blobStreamer = blobStreamer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rpc Server: Started");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rpc Server: Stopped");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Attempt to retreive and stream a Download Path to Blob Storage.
        /// </summary>
        protected override string OnMessageReceived(long matchId, string response)
        {
            var message_model = JsonConvert.DeserializeObject<DC_DD_Model>(response);

            _logger.LogInformation(
                $"Match {matchId}: Received DownloadPath: [ {message_model.DownloadPath} ]");

            var model = new DD_DC_Model
            {
                matchId = matchId,
            };

            try
            {
                model.zippedFilePath = _blobStreamer.StreamToBlobAsync(
                    message_model.DownloadPath).GetAwaiter().GetResult();

                model.Success = true;
            }
            catch (Exception e){
                _logger.LogError($"Match {matchId}: Streaming failed with: {e.Message}");

                model.Success = false;
            }

            string model_json = JsonConvert.SerializeObject(model);
            _logger.LogInformation(model_json);

            return model_json;
        }
    }
}
