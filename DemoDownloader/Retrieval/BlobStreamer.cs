﻿using DemoDownloader.Storage;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DemoDownloader.Retrieval
{
    public class BlobStreamer
    {
        private readonly ILogger<BlobStreamer> _logger;
        private readonly BlobStorage _blobStorage;
        HttpClient httpClient;

        public BlobStreamer(ILogger<BlobStreamer> logger, BlobStorage blobStorage)
        {
            _logger = logger;
            _blobStorage = blobStorage;
            httpClient = new HttpClient();
        }

        public async Task<string> StreamToBlobAsync(string fileUrl)
        {
            string blob_id = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = _blobStorage.CloudBlobContainer.GetBlockBlobReference(blob_id);

            _logger.LogInformation($"Attempting download from [ {fileUrl} ]");

            var response = await httpClient.GetAsync(
                fileUrl, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await blockBlob.UploadFromStreamAsync(source: stream);
                }
                _logger.LogInformation($"Retrieved and Uploaded [ {blob_id} ]");
            }
            else
            {
                _logger.LogError($"{fileUrl} Download failed with: [ {response.StatusCode}: {(int)response.StatusCode} ]");
                response.EnsureSuccessStatusCode();
            }

            return blockBlob.Uri.ToString();
        }


    }
}