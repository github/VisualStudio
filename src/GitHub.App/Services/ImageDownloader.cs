using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        readonly IDictionary<string, NonImageContentException> exceptionCache;

        [ImportingConstructor]
        public ImageDownloader(Lazy<IHttpClient> httpClient)
        {
            this.httpClient = httpClient;
            exceptionCache = new Dictionary<string, NonImageContentException>();
        }

        public static string CachedExceptionMessage(string host) =>
            string.Format(CultureInfo.InvariantCulture, "Host '{0}' previously returned a non-image content type", host);
        public static string CouldNotDownloadExceptionMessage(Uri imageUri) =>
            string.Format(CultureInfo.InvariantCulture, "Could not download image from '{0}'", imageUri);
        public static string NonImageContentExceptionMessage(string contentType) =>
            string.Format(CultureInfo.InvariantCulture, "Server responded with a non-image content type '{0}'", contentType);

        /// <summary>
        /// Get the bytes for a given image URI.
        /// </summary>
        /// <remarks>
        /// If a host returns a non-image content type, this will be remembered and subsequent download requests
        /// to the same host will automatically throw a <see cref="NonImageContentException"/>. This prevents a
        /// barrage of download requests when authentication is required (but not currently supported).
        /// </remarks>
        /// <param name="imageUri">The URI of an image.</param>
        /// <returns>The bytes for a given image URI.</returns>
        /// <exception cref="HttpRequestException">When the URI returns a status code that isn't OK/200.</exception>
        /// <exception cref="NonImageContentException">When the URI returns a non-image content type.</exception>
        public IObservable<byte[]> DownloadImageBytes(Uri imageUri)
        {
            return ExceptionCachingDownloadImageBytesAsync(imageUri).ToObservable();
        }

        async Task<byte[]> ExceptionCachingDownloadImageBytesAsync(Uri imageUri)
        {
            var host = imageUri.Host;

            NonImageContentException exception;
            if (exceptionCache.TryGetValue(host, out exception))
            {
                throw new NonImageContentException(CachedExceptionMessage(host), exception);
            }

            try
            {
                return await DownloadImageBytesAsync(imageUri);
            }
            catch (NonImageContentException e)
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
                throw new NonImageContentException(NonImageContentExceptionMessage(response.ContentType));
            }

            return response.Body as byte[];
        }

        IHttpClient HttpClient { get { return httpClient.Value; } }
    }

    [Serializable]
    public class NonImageContentException : HttpRequestException
    {
        public NonImageContentException() { }
        public NonImageContentException(string message) : base(message) { }
        public NonImageContentException(string message, Exception inner) : base(message, inner) { }

        [SuppressMessage("Microsoft.Usage", "CA2236:CallBaseClassMethodsOnISerializableTypes",
            Justification = "HttpRequestException doesn't have required constructor")]
        protected NonImageContentException(SerializationInfo info, StreamingContext context) { }
    }
}
