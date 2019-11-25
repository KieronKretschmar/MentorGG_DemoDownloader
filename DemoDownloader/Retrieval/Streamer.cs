using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DemoDownloader.Retrieval
{
    interface IStreamer
    {
        public bool AttemptStream();
    }

    class Streamer
    {
        CloudStorageAccount cloudStorageAccount;
        CloudBlobClient cloudBlobClient;

        public Streamer()
        {
            cloudStorageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true;");
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        }

        public async Task AttemptStream()
        {
            var c_name = "test-" + Guid.NewGuid();
            var container = cloudBlobClient.GetContainerReference(c_name);

            try
            {
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                await container.CreateIfNotExistsAsync(requestOptions, null);
            }
            catch (StorageException)
            {
                Console.WriteLine(
                    "If you are running with the default connection string, please make sure you have started the storage emulator");
                Console.ReadLine();
                throw;
            }

            string fileUrl = "https://demos-europe-west2.faceit-cdn.net/csgo/418fc933-9f72-418e-815f-566c86f125e0.dem.gz";

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("demo");

            DownloadClient client = DownloadClient.FromTimespan(TimeSpan.FromSeconds(30));

            Stream stream;
            using (client)
            {
                stream = client.OpenRead(new Uri(fileUrl));
            }
            await blockBlob.UploadFromStreamAsync(stream);

            foreach (IListBlobItem blob in container.ListBlobs())
            {
                Console.WriteLine("- {0} (type: {1})", blob.Uri, blob.GetType());
            }

        }
    }
}
