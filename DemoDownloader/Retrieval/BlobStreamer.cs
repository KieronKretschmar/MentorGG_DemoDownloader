using DemoDownloader.Storage;
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
using System.Text.RegularExpressions;
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
            var extension = GetFullFileEnding(fileUrl);
            string blob_id = $"{Guid.NewGuid()}{extension}";
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
                _logger.LogInformation($"Retrieved Url [ {fileUrl} ] and Uploaded [ {blob_id} ]");
            }
            else
            {
                var msg = $"{fileUrl} Download failed with: [ {response.StatusCode}: {(int)response.StatusCode} ]";
                _logger.LogError(msg);
                throw new HttpRequestException(msg);
            }

            return blockBlob.Uri.ToString();
        }


        /// <summary>
        /// Returns the full file ending of the filePath, including double endings like ".dem.gz".
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetFullFileEnding(string filePath)
        {
            return Regex.Match(filePath, @"\..*").Value;
        }
    }
}