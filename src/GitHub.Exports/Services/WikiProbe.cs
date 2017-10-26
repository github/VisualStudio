using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using GitHub.Models;
using Octokit;
using Octokit.Internal;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;

namespace GitHub.Services
{
    /// <summary>
    /// Since VS doesn't support dynamic component registration, we have to implement wrappers
    /// for types we don't control in order to export them.
    /// </summary>
    [Export(typeof(IHttpClient))]
    public class ExportedHttpClient : HttpClientAdapter
    {
        public ExportedHttpClient() :
            base(HttpMessageHandlerFactory.CreateDefault)
        {}
    }

    [Export(typeof(IWikiProbe))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WikiProbe : IWikiProbe
    {
        readonly ProductHeaderValue productHeader;
        readonly IHttpClient httpClient;

        [ImportingConstructor]
        public WikiProbe(IProgram program, IHttpClient httpClient)
        {
            this.productHeader = program.ProductHeader;
            this.httpClient = httpClient;
        }

        public async Task<WikiProbeResult> ProbeAsync(Repository repo)
        {
            var repoUri = new Uri(repo.HtmlUrl);
            var baseUri = new Uri(repo.HtmlUrl.Replace(repoUri.AbsolutePath, ""));
            var request = new Request
            {
                Method = HttpMethod.Get,
                BaseAddress = baseUri,
                Endpoint = new Uri(repoUri.AbsolutePath + "/wiki", UriKind.Relative),
                Timeout = TimeSpan.FromSeconds(3),
            };
            request.Headers.Add("User-Agent", productHeader.ToString());

            var ret = await httpClient
                .Send(request, CancellationToken.None)
                .Catch();

            if (ret == null)
                return WikiProbeResult.Failed;
            else if (ret.StatusCode == System.Net.HttpStatusCode.OK)
                return WikiProbeResult.Ok;
            return WikiProbeResult.NotFound;
        }
    }

    public enum WikiProbeResult
    {
        /// <summary>
        /// There is a wiki
        /// </summary>
        Ok,

        /// <summary>
        /// There is no wiki
        /// </summary>
        NotFound,

        /// <summary>
        /// The existence of a wiki is uncertain.
        /// Request timed out or DNS failed.
        /// </summary>
        Failed
    }
}
