using Microsoft.Extensions.Configuration;
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
    public interface IDownloader
    {
        /// <summary>
        /// Attempt to download a file
        /// </summary>
        /// <param name="url">Download Url</param>
        /// <param name="outputFilePath">Path where the successful file is output to</param>
        /// <returns>Indication of the download success</returns>
        bool AttemptDownload(string url, out string outputFilePath);
    }

    public class Downloader : IDownloader
    {
        static readonly Stopwatch Timer = new Stopwatch();
        static readonly char[] UrlSeperators = { '/', '\\' };
        static readonly string DefaultDemoDirectory = Path.GetFullPath("/Demos");

        static string DemoDirectory;

        /// <summary>
        /// Set the Demo Directory
        /// </summary>
        public Downloader(IConfiguration configuration)
        {
            string enviromentDemoDirectory = configuration.GetSection("DEMO_DIRECTORY").Value;
            DemoDirectory = enviromentDemoDirectory ?? DefaultDemoDirectory;
            Directory.CreateDirectory(DemoDirectory);
        }

        /// <summary>
        /// Attempt to download a file
        /// </summary>
        /// <param name="url">Download Url</param>
        /// <param name="outputFilePath">Path where the successful file is output to</param>
        /// <returns>Indication of the download success</returns>
        public bool AttemptDownload(string url, out string outputFilePath)
        {
            string urlFileName = url.Split(UrlSeperators).Last();
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(urlFileName);
            outputFilePath = Path.Join(DemoDirectory, uniqueFileName);

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
                    $"Thread: {Thread.CurrentThread.ManagedThreadId} :: " +
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
}
