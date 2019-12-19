﻿using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DemoDownloader.Retrieval;
using Microsoft.Extensions.Logging;
using RabbitTransfer.Interfaces;
using RabbitTransfer.Consumer;
using System.Net.Http;
using RabbitTransfer.Producer;
using RabbitTransfer.TransferModels;
using RabbitTransfer.RPC;

namespace DemoDownloader.RPC
{
    public class UrlConsumer : RPCServer<DC_DD_Model, DD_DC_Model>
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
        public override DD_DC_Model HandleMessageAndReply(IBasicProperties properties, DC_DD_Model consumeModel)
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
            catch (HttpRequestException e){
                _logger.LogError($"Match {matchId}: HttpRequestException: {e.Message}");
                produceModel.Success = false;
            }
            catch (Exception e)
            {
                _logger.LogError($"Match {matchId}: Unknown Exception: {e.Message}");
                produceModel.Success = false;
            }

            return produceModel;
        }
    }
}
