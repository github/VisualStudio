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

        [Test]
        public void NotFoundTwiceForSameHost_ThrowsCachedHttpRequestException()
        {

            var host = "flaky404.githubusercontent.com";
            var url = new Uri("https://" + host + "/u/00000000?v=4");
            var expectMessage = ImageDownloader.CachedExceptionMessage(host);
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.NotFound);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            Assert.ThrowsAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));
            var ex = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));

            Assert.That(ex.Message, Is.EqualTo(expectMessage));
        }

        [Test]
        public void NotFoundTwiceForDifferentHosts_DoesNotThrowCachedHttpRequestException()
        {
            var url1 = new Uri("https://host1/u/00000000?v=4");
            var url2 = new Uri("https://host2/u/00000000?v=4");
            var expectMessage1 = ImageDownloader.CouldNotDownloadExceptionMessage(url1);
            var expectMessage2 = ImageDownloader.CouldNotDownloadExceptionMessage(url2);
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.NotFound);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            var ex1 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url1));
            var ex2 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url2));

            Assert.That(ex1?.Message, Is.EqualTo(expectMessage1));
            Assert.That(ex2?.Message, Is.EqualTo(expectMessage2));
        }
    }
}
