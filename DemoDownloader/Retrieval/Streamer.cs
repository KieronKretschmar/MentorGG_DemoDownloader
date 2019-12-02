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
    public interface IBlobStreamer
    {
        Task StreamToBlobAsync(string fileUrl);
    }

    class Streamer : IBlobStreamer
    {
        ILogger<Streamer> logger;
        CloudBlobContainer blobContainer;
        HttpClient httpClient;

        public Streamer(IBlobStorage storage, ILogger<Streamer> logger)
        {
            blobContainer = storage.CloudBlobContainer;
            this.logger = logger;
            httpClient = new HttpClient();
        }

        public async Task StreamToBlobAsync(string fileUrl)
        {
            string blob_id = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blob_id);

            logger.LogInformation($"Downloading from {fileUrl} => {blobContainer.Name}.{blob_id}.");

            Stream stream = await httpClient.GetStreamAsync(fileUrl).ContinueWith(task =>
            {
                try
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        return task.Result;
                    }
                    else
                    {
                        throw task.Exception;
                    }
                }
                catch (AggregateException ex)
                {
                    logger.LogError(ex.GetBaseException(), "Caught AggregateException");
                    throw ex;
                }
                catch (WebException ex)
                {
                    logger.LogError(ex, "Caught WebException");
                    throw ex;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled Exception");
                    throw ex;
                }
            });

            logger.LogInformation($"Retrieved {blob_id}. {stream.Length}");

            await blockBlob.UploadFromStreamAsync(source: stream);

            logger.LogInformation($"Uploaded {blob_id}.");

            foreach (IListBlobItem blob in blobContainer.ListBlobs())
            {
                logger.LogInformation("- {0} (type: {1})", blob.Uri, blob.GetType());
            }
        }


    }
}