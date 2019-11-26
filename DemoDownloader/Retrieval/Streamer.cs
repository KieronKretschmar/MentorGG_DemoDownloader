using DemoDownloader.Storage;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
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
        CloudBlobContainer blobContainer;
        HttpClient httpClient;

        public Streamer(IBlobStorage storage)
        {
            blobContainer = storage.CloudBlobContainer;
            httpClient = new HttpClient();
        }

        public async Task StreamToBlobAsync(string fileUrl)
        {
            string blob_id = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blob_id);

            Console.WriteLine($"Downloading from {fileUrl} => {blobContainer.Name}.{blob_id}.");
            Stream stream;
            try
            {
                stream = await httpClient.GetStreamAsync(fileUrl);
            }
            catch (Exception e)
            {
                throw e;
            }

            Console.WriteLine($"Retrieved {blob_id}.");

            await blockBlob.UploadFromStreamAsync(source: stream);

            Console.WriteLine($"Uploaded {blob_id}.");

            foreach (IListBlobItem blob in blobContainer.ListBlobs())
            {
                Console.WriteLine("- {0} (type: {1})", blob.Uri, blob.GetType());
            }
        }


    }
}
