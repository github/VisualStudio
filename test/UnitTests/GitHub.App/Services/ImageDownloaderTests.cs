using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using GitHub.Services;
using Octokit;
using Octokit.Internal;
using NSubstitute;
using NUnit.Framework;

public class ImageDownloaderTests
{
    public class TheDownloadImageBytesMethod
    {
        [Test]
        public async Task HttpStatusCode_OK()
        {
            var url = new Uri("http://foo.bar");
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);
            response.ContentType.Returns("image/xxx");
            response.Body.Returns(Array.Empty<byte>());
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            var bytes = await target.DownloadImageBytes(url);

            Assert.IsEmpty(bytes);
        }

        [Test]
        public void ContentTypeText_ThrowsHttpRequestException()
        {
            var url = new Uri("http://foo.bar");
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);
            response.ContentType.Returns("text/plain");
            response.Body.Returns(Array.Empty<byte>());
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            Assert.ThrowsAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));
        }

        [Test]
        public void HttpStatusCode_NotFound404_ThrowsHttpRequestException()
        {
            var url = new Uri("http://foo.bar");
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.NotFound);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            Assert.ThrowsAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));
        }
    }
}
