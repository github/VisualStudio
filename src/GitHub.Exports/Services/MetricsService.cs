#define SEND_DEBUG_METRICS
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using Octokit.Internal;

namespace GitHub.Services
{
    [Export(typeof(IMetricsService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MetricsService : IMetricsService
    {
#if DEBUG
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "We have conditional compilation")]
        static readonly Uri centralUri = new Uri("http://localhost:4000/", UriKind.Absolute);
#else
        static readonly Uri centralUri = new Uri("https://central.github.com/", UriKind.Absolute);
#endif

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "We have conditional compilation")]
        readonly Lazy<IHttpClient> httpClient;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "We have conditional compilation")]
        readonly ProductHeaderValue productHeader;

        [ImportingConstructor]
        public MetricsService(Lazy<IHttpClient> httpClient, IProgram program)
        {
            this.httpClient = httpClient;
            this.productHeader = program.ProductHeader;
        }

#if DEBUG && !SEND_DEBUG_METRICS
        public Task PostUsage(UsageModel model)
        {
            return Task.CompletedTask;
        }
#else
        public async Task PostUsage(UsageModel model)
        {
            var request = new Request
            {
                Method = HttpMethod.Post,
                BaseAddress = centralUri,
                Endpoint = new Uri("api/usage/visualstudio", UriKind.Relative),
            };

            request.Headers.Add("User-Agent", productHeader.ToString());

            request.Body = model.SerializeForRequest();
            request.ContentType = "application/json";

            await httpClient.Value.Send(request);
        }
#endif

#if DEBUG && !SEND_DEBUG_METRICS
        public Task PostOptIn(bool optIn)
        {
            return Task.CompletedTask;
        }
#else
        public async Task PostOptIn(bool optIn)
        {
            var request = new Request
            {
                Method = HttpMethod.Post,
                BaseAddress = centralUri,
                Endpoint = new Uri("api/usage/visualstudio", UriKind.Relative),
            };

            request.Headers.Add("User-Agent", productHeader.ToString());

            request.Body = new { EventType = "ping", OptIn = optIn }.SerializeForRequest();
            request.ContentType = "application/json";

            await httpClient.Value.Send(request);
        }
#endif

        public Task SendOptOut()
        {
            // Temporarily disabled until https://github.com/github/central/issues/213 gets resolved
            return Task.FromResult(0);

            /*
            var request = new Request
            {
                Method = HttpMethod.Post,
                AllowAutoRedirect = true,
                Endpoint = new Uri(centralUri, new Uri("api/usage/visualstudio?optout=1", UriKind.Relative)),
            };
            request.Headers.Add("User-Agent", productHeader.ToString());
            request.Body = new StringContent("");
            request.ContentType = "application/json";
            await httpClient.Value.Send((IRequest)request, cancellationToken);
             */
        }

        public Task SendOptIn()
        {
            // Temporarily disabled until https://github.com/github/central/issues/213 gets resolved
            return Task.FromResult(0);

            /*
            var request = new Request
            {
                Method = HttpMethod.Post,
                AllowAutoRedirect = true,
                Endpoint = new Uri(centralUri, new Uri("api/usage/visualstudio?optin=1", UriKind.Relative)),
            };
            request.Headers.Add("User-Agent", productHeader.ToString());
            request.Body = new StringContent("");
            request.ContentType = "application/json";
            return Observable.FromAsync(cancellationToken => httpClient.Value.Send((IRequest)request, cancellationToken))
                .AsCompletion();
             */
        }
    }
}
