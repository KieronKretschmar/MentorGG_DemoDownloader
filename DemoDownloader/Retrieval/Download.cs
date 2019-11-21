using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace DemoDownloader.Retrieval
{
    public static class Download
    {
        static readonly Stopwatch Timer = new Stopwatch();
        static readonly char[] UrlSeperators = { '/', '\\' };

        static readonly string DefaultDownloadDirectory = Path.GetFullPath("/Users/Arran/Downloads/Demos");
        static readonly string DownloadDirectory;

        /// <summary>
        /// Static constructor
        /// </summary>
        static Download()
        {
            DownloadDirectory = Environment.GetEnvironmentVariable(
                "DOWNLOAD_DIRECTORY") ?? DefaultDownloadDirectory;
            Directory.CreateDirectory(DownloadDirectory);
        }

        /// <summary>
        /// Attempt to download a file
        /// </summary>
        /// <param name="url">Download Url</param>
        /// <param name="outputFilePath">Path where the successful file is output to</param>
        /// <returns></returns>
        public static bool AttemptDownload(string url, out string outputFilePath)
        {
            string urlFileName = url.Split(UrlSeperators).Last();
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(urlFileName);
            outputFilePath = Path.Join(DownloadDirectory, uniqueFileName);

            Console.WriteLine(
                $"Thread: {Thread.CurrentThread.ManagedThreadId} :: " +
                $"Attempting to download `{urlFileName}` from `{url}`." +
                $"Output File Path: `{outputFilePath}`");

            DownloadClient client = DownloadClient.FromTimespan(TimeSpan.FromSeconds(30));

            Timer.Start();
            try
            {
                using (client)
                {
                    client.DownloadFile(url, outputFilePath);
                }

                Console.WriteLine(
                    $"Succesful download from `{url}`, " +
                    $"Time elaped: {Timer.Elapsed}");

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
            finally
            {
                Timer.Reset();
            }
        }
    }

    public class DownloadClient : WebClient
    {
        /// <summary>
        /// HttpRequest timeout in milliseconds
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        /// Initialize a WebClient
        /// </summary>
        /// <param name="timeout">Timeout as a TimeSpan</param>
        public static DownloadClient FromTimespan(TimeSpan timeout)
        {
            return new DownloadClient((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Initialize a WebClient
        /// </summary>
        /// <param name="timeout">Timeout in Milliseconds</param>
        public DownloadClient(int timeout)
        {
            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeout must be greater than 0 milliseconds");
            }
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            if (request != null)
            {
                request.Timeout = Timeout;
            }
            return request;
        }
    }
}
