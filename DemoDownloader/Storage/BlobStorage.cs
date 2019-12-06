﻿using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoDownloader.Storage
{
    public class BlobStorage
    {
        ILogger<BlobStorage> logger;
        CloudStorageAccount cloudStorageAccount;
        CloudBlobClient cloudBlobClient;

        public CloudBlobContainer CloudBlobContainer { get; private set; }

        /// <summary>
        /// Connects to blob storage and creates a blob container.
        /// </summary>
        /// <param name="configuration"></param>
        public BlobStorage(IConfiguration configuration, ILogger<BlobStorage> logger)
        {
            string containerReference = configuration.GetSection(
                "BLOB_CONTAINER_REF").Value ?? "demos";
            string connectionString = configuration.GetSection(
                "BLOB_CONNECTION_STRING").Value ?? "UseDevelopmentStorage=true;";

            this.logger = logger;

            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer = GetBlobContainer(containerReference);
        }


        /// <summary>
        /// Returns a CloudBlobContainer, creating if it does not exist.
        /// </summary>
        /// <param name="containerReference">Reference name of the blob container</param>
        /// <returns>CloudBlobContainer</returns>
        private CloudBlobContainer GetBlobContainer(string containerReference)
        {
            var container = cloudBlobClient.GetContainerReference(containerReference);
            try
            {
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                container.CreateIfNotExists(requestOptions, null);
            }
            catch (StorageException exception)
            {
                logger.LogWarning(
                    "If you are running with the default connection string," +
                    " please make sure you have started the storage emulator");
                throw exception;
            }

            return container;
        }
    }
}
