using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHub.Models;
using System.Threading.Tasks;
using System.Net;

namespace MetricsTests
{
    [TestFixture]
    public class Submissions
    {
        HttpClient client;
        Uri uri;
        MetricsServer.Server server;

        [OneTimeSetUp]
        public void Setup()
        {
            uri = new Uri("http://localhost:4000");
            if (uri.Host == "localhost")
            {
                server = new MetricsServer.Server(uri.Host, uri.Port);
                server.Start();
            }

            client = new HttpClient();
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "/api/usage/visualstudio"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            server?.Stop();
        }

        [Test]
        public async Task ValidDimensions()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "/api/usage/visualstudio"));
            var data = new UsageData();
            data.Reports = new List<UsageModel> { UsageModel.Create(Guid.NewGuid()) };
            var model = data.Reports[0];
            model.Dimensions.AppVersion = "9.9.9";
            model.Dimensions.Lang = "en-us";
            model.Dimensions.VSVersion = "14";
            model.Measures.NumberOfStartups = 1;

            request.Content = GitHub.Services.MetricsService.SerializeRequest(model);

            HttpResponseMessage response = null;
            Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
            var ret = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task InvalidAppVersion()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "/api/usage/visualstudio"));
            var data = new UsageData();
            data.Reports = new List<UsageModel> { UsageModel.Create(Guid.NewGuid()) };
            var model = data.Reports[0];
            model.Dimensions.AppVersion = "nope";
            model.Dimensions.Lang = "en-us";
            model.Dimensions.VSVersion = "14";
            model.Measures.NumberOfStartups = 1;

            request.Content = GitHub.Services.MetricsService.SerializeRequest(model);

            HttpResponseMessage response = null;
            Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
            var ret = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
