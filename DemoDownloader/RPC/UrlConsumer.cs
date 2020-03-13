using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using RabbitCommunicationLib.RPC;
using RabbitCommunicationLib.Interfaces;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitCommunicationLib.TransferModels;
using RabbitCommunicationLib.Enums;

namespace DemoDownloader.RPC
{
    public class UrlConsumer : RPCServer<DemoDownloadInstruction, DemoObtainReport>
    {
        private readonly ILogger<UrlConsumer> _logger;
        private readonly BlobStreamer _blobStreamer;

        /// <summary>
        /// Attach a Logger and Blob Streamer
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="blobStreamer"></param>
        public UrlConsumer(
            ILogger<UrlConsumer> logger,
            BlobStreamer blobStreamer,
            IRPCQueueConnections queueConnections) : base(queueConnections)
        {
            _logger = logger;
            _blobStreamer = blobStreamer;
        }

        /// <summary>
        /// Attempt to retreive and stream a Download Path to Blob Storage.
        /// </summary>
        public override async Task<ConsumedMessageHandling<DemoObtainReport>> HandleMessageAndReplyAsync(BasicDeliverEventArgs ea, DemoDownloadInstruction consumeModel)
        {
            long matchId = consumeModel.MatchId;

            _logger.LogInformation(
                $"Match {matchId}: Received Download Url: [ {consumeModel.DownloadUrl} ]");

            var produceModel = new DemoObtainReport();

            try
            {
                produceModel.MatchId = matchId;
                produceModel.BlobUrl = await _blobStreamer.StreamToBlobAsync(
                    consumeModel.DownloadUrl);

                produceModel.Success = true;
            }
            catch (HttpRequestException e){
                _logger.LogError($"Match {matchId}: HttpRequestException: {e.Message}");
                produceModel.Success = false;
            }
            catch (Exception e)
            {
                _logger.LogError($"Match {matchId}: Unknown Exception: {e.Message}");
                produceModel.Success = false;
            }

            _logger.LogInformation($"Match {matchId}: Done");

            var reply = new ConsumedMessageHandling<DemoObtainReport>();
            reply.MessageHandling = ConsumedMessageHandling.Done;
            reply.TransferModel = produceModel;
            
            return reply;
        }
    }
}
