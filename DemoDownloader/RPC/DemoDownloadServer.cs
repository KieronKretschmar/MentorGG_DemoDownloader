using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Logging;
using RabbitTransfer.Interfaces;
using RabbitTransfer.Consumer;

namespace DemoDownloader.RPC
{
    public class DemoDownloadServer : RPCConsumer<DC_DD_Model, DD_DC_Model>
    {
        private readonly ILogger<DemoDownloadServer> _logger;
        private readonly BlobStreamer _blobStreamer;

        /// <summary>
        /// Attach a Logger and Blob Streamer
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="blobStreamer"></param>
        public DemoDownloadServer(
            ILogger<DemoDownloadServer> logger,
            BlobStreamer blobStreamer,
            DownloadQueueConnection queueConnection) : base(queueConnection)
        {
            _logger = logger;
            _blobStreamer = blobStreamer;
        }

        /// <summary>
        /// Attempt to retreive and stream a Download Path to Blob Storage.
        /// </summary>
        protected override DD_DC_Model HandleMessageAndReply(IBasicProperties properties, DC_DD_Model consumeModel)
        {
            var matchId = long.Parse(properties.CorrelationId);

            _logger.LogInformation(
                $"Match {matchId}: Received Download Url: [ {consumeModel.DownloadUrl} ]");

            var produceModel = new DD_DC_Model();

            try
            {
                produceModel.DemoUrl = _blobStreamer.StreamToBlobAsync(
                    consumeModel.DownloadUrl).GetAwaiter().GetResult();

                produceModel.Success = true;
            }
            catch (Exception e){
                _logger.LogError($"Match {matchId}: Streaming failed with: {e.Message}");

                produceModel.Success = false;
            }

            return produceModel;
        }
    }
}
