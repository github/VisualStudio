using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using GitHub.Models;
using Octokit;
using Octokit.Internal;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Threading.Tasks;
using System.Net;
using System.Threading.Tasks;

namespace GitHub.Services
{
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

        public IObservable<WikiProbeResult> Probe(Repository repo)
        {
            var repoUri = new Uri(repo.HtmlUrl);
            var baseUri = new Uri(repo.HtmlUrl.Replace(repoUri.AbsolutePath, ""));
            var request = new Request
            {
                Method = HttpMethod.Get,
                BaseAddress = baseUri,
                Endpoint = new Uri(repoUri.AbsolutePath + "/wiki", UriKind.Relative),
                Timeout = TimeSpan.FromSeconds(3),
                AllowAutoRedirect = false,
            };
            request.Headers.Add("User-Agent", productHeader.ToString());

            return httpClient.Send<object>(request, CancellationToken.None)
                .ToObservable()
                .Catch(Observable.Return<IResponse<object>>(null))
                .Select(resp => resp == null
                    ? WikiProbeResult.Failed
                    : (resp.StatusCode == HttpStatusCode.OK
                        ? WikiProbeResult.Ok
                        : WikiProbeResult.NotFound));
        }

        public async Task<WikiProbeResult> AsyncProbe(Repository repo)
        {
            return await Probe(repo).FirstAsync();
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
