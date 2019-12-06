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
using System.Threading.Tasks;

namespace DemoDownloader.Retrieval
{
    public class BlobStreamer
    {
        ILogger<BlobStreamer> logger;
        CloudBlobContainer blobContainer;
        HttpClient httpClient;

        public BlobStreamer(ILogger<BlobStreamer> logger, BlobStorage storage)
        {
            this.logger = logger;
            blobContainer = storage.CloudBlobContainer;
            httpClient = new HttpClient();
        }

        public async Task StreamToBlobAsync(string fileUrl)
        {
            string blob_id = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blob_id);

            logger.LogInformation($"Attempting download from {fileUrl}.");

            var response = await httpClient.GetAsync(
                fileUrl, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await blockBlob.UploadFromStreamAsync(source: stream);
                }
                logger.LogInformation($"\nRetrieved and Uploaded {blob_id}");
            }
            else
            {
                var msg = $"{fileUrl} Download failed with {response.StatusCode}";
                logger.LogError(msg);
                throw new Exception(msg);
            }

            var blobList = new List<string>();
            foreach (IListBlobItem blob in blobContainer.ListBlobs())
            {
                blobList.Add($"{blob.Uri} (type: {blob.GetType()}");
            }

            logger.LogInformation(string.Join("\n -", blobList));
        }


    }
}