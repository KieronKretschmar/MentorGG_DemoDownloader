using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitTransfer;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Logging;
using RabbitMQ.Abstract;

namespace DemoDownloader.RPC
{
    public class DemoDownloadServer : RPCServerService
    {
        private readonly ILogger<DemoDownloadServer> _logger;
        private readonly BlobStreamer _blobStreamer;

        public override string QueueName => RPCExchange.DC_DD.QUEUE;

        /// <summary>
        /// Attach a Logger and Blob Streamer
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="blobStreamer"></param>
        public DemoDownloadServer(
            ILogger<DemoDownloadServer> logger,
            BlobStreamer blobStreamer,
            IConnection connection) : base(connection)
        {
            this._logger = logger;
            this._blobStreamer = blobStreamer;
        }

        /// <summary>
        /// Attempt to retreive and stream a Download Path to Blob Storage.
        /// </summary>
        protected override string HandleMessageRecieved(long matchId, string response)
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
