using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Logging;
using RabbitTransfer.Interfaces;
using RabbitTransfer.Consumer;

namespace DemoDownloader.RPC
{
    public class DemoDownloadServer : RPCConsumer
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
        protected override string HandleMessageAndReply(IBasicProperties properties, string message)
        {
            var matchId = long.Parse(properties.CorrelationId);
            var message_model = JsonConvert.DeserializeObject<DC_DD_Model>(message);

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

            var replyMessage = JsonConvert.SerializeObject(model);
            _logger.LogInformation(replyMessage);

            return replyMessage;
        }
    }
}
