using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DemoDownloader.Retrieval
{
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
                ((HttpWebRequest)request).ReadWriteTimeout = Timeout;
            }
            return request;
        }
    }
}
