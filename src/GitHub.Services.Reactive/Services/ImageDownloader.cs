using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using GitHub.Logging;
using Octokit;
using Octokit.Internal;

namespace GitHub.Services
{
    [Export(typeof(IImageDownloader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageDownloader : IImageDownloader
    {
        readonly Lazy<IHttpClient> httpClient;

        [ImportingConstructor]
        public ImageDownloader(Lazy<IHttpClient> httpClient)
        {
            this.httpClient = httpClient;
        }

        public IObservable<byte[]> DownloadImageBytes(Uri imageUri)
        {
            var request = new Request
            {
                BaseAddress = new Uri(imageUri.GetLeftPart(UriPartial.Authority)),
                Endpoint = new Uri(imageUri.PathAndQuery, UriKind.RelativeOrAbsolute),
                Method = HttpMethod.Get,
            };

            return HttpClient.Send(request)
                .ToObservable()
                .Select(response => GetSuccessfulBytes(imageUri, response));
        }

        static byte[] GetSuccessfulBytes(Uri imageUri, IResponse response)
        {
            Log.Assert(imageUri != null, "The imageUri cannot be null");
            Log.Assert(response != null, "The response cannot be null");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(string.Format(CultureInfo.InvariantCulture, "Could not download image from {0}", imageUri));
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