using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Logging;
using Octokit;
using Octokit.Internal;
using System.Collections.Generic;

namespace GitHub.Services
{
    [Export(typeof(IImageDownloader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageDownloader : IImageDownloader
    {
        readonly Lazy<IHttpClient> httpClient;
        readonly IDictionary<string, Exception> exceptionCache;

        [ImportingConstructor]
        public ImageDownloader(Lazy<IHttpClient> httpClient)
        {
            this.httpClient = httpClient;
            exceptionCache = new Dictionary<string, Exception>();
        }

        public static string CachedExceptionMessage(string host) =>
            "Throwing cached exception for host: " + host;
        public static string CouldNotDownloadExceptionMessage(Uri imageUri) =>
            string.Format(CultureInfo.InvariantCulture, "Could not download image from {0}", imageUri);

        public IObservable<byte[]> DownloadImageBytes(Uri imageUri)
        {
            return ExceptionCachingDownloadImageBytesAsync(imageUri).ToObservable();
        }

        async Task<byte[]> ExceptionCachingDownloadImageBytesAsync(Uri imageUri)
        {
            var host = imageUri.Host;

            Exception exception;
            if (exceptionCache.TryGetValue(host, out exception))
            {
                throw new HttpRequestException(CachedExceptionMessage(host), exception);
            }

            try
            {
                return await DownloadImageBytesAsync(imageUri);
            }
            catch (Exception e)
            {
                exceptionCache[host] = e;
                throw;
            }
        }

        async Task<byte[]> DownloadImageBytesAsync(Uri imageUri)
        {
            var request = new Request
            {
                BaseAddress = new Uri(imageUri.GetLeftPart(UriPartial.Authority)),
                Endpoint = new Uri(imageUri.PathAndQuery, UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Get,
            };

            var response = await HttpClient.Send(request);
            return GetSuccessfulBytes(imageUri, response);
        }

        static byte[] GetSuccessfulBytes(Uri imageUri, IResponse response)
        {
            Log.Assert(imageUri != null, "The imageUri cannot be null");
            Log.Assert(response != null, "The response cannot be null");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(CouldNotDownloadExceptionMessage(imageUri));
            }

            if (response.ContentType == null || !response.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpRequestException(
                    string.Format(CultureInfo.InvariantCulture,
                        "Server responded with a non-image content type: {0}", response.ContentType));
            }

            return response.Body as byte[];
        }

        IHttpClient HttpClient { get { return httpClient.Value; } }
    }
}