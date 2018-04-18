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

            Assert.ThrowsAsync<NonImageContentException>(async () => await target.DownloadImageBytes(url));
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
        public void NotFoundTwiceForSameHost_CouldNotDownloadExceptionMessage()
        {
            var host = "flaky404.githubusercontent.com";
            var url = new Uri("https://" + host + "/u/00000000?v=4");
            var expectMessage = ImageDownloader.CouldNotDownloadExceptionMessage(url);
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.NotFound);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            var ex1 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));
            var ex2 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));

            Assert.That(ex1?.Message, Is.EqualTo(expectMessage));
            Assert.That(ex2?.Message, Is.EqualTo(expectMessage));
        }

        [Test]
        public void NonImageContentForSameHost_ThrowsCachedHttpRequestException()
        {
            var host = "host";
            var url = new Uri("https://" + host + "/image");
            var contentType = "text/html";
            var expectMessage1 = ImageDownloader.NonImageContentExceptionMessage(contentType);
            var expectMessage2 = ImageDownloader.CachedExceptionMessage(host);
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);
            response.ContentType.Returns(contentType);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            var ex1 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));
            var ex2 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url));

            Assert.That(ex1?.Message, Is.EqualTo(expectMessage1));
            Assert.That(ex2?.Message, Is.EqualTo(expectMessage2));
        }

        [Test]
        public void NonImageContentForDifferentHosts_DoesNotThrowCachedHttpRequestException()
        {
            var url1 = new Uri("https://host1/image");
            var url2 = new Uri("https://host2/image");
            var contentType = "text/html";
            var expectMessage1 = ImageDownloader.NonImageContentExceptionMessage(contentType);
            var expectMessage2 = ImageDownloader.NonImageContentExceptionMessage(contentType);
            var httpClient = Substitute.For<IHttpClient>();
            var response = Substitute.For<IResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);
            response.ContentType.Returns(contentType);
            httpClient.Send(null, default(CancellationToken)).ReturnsForAnyArgs(Task.FromResult(response));
            var target = new ImageDownloader(new Lazy<IHttpClient>(() => httpClient));

            var ex1 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url1));
            var ex2 = Assert.CatchAsync<HttpRequestException>(async () => await target.DownloadImageBytes(url2));

            Assert.That(ex1?.Message, Is.EqualTo(expectMessage1));
            Assert.That(ex2?.Message, Is.EqualTo(expectMessage2));
        }
    }
}
