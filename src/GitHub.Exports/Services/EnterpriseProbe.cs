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
    [Export(typeof(IEnterpriseProbe))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EnterpriseProbe : IEnterpriseProbe
    {
        static readonly Uri endPoint = new Uri("/site/sha", UriKind.Relative);
        readonly ProductHeaderValue productHeader;
        readonly IHttpClient httpClient;

        [ImportingConstructor]
        public EnterpriseProbe(IProgram program, IHttpClient httpClient)
        {
            productHeader = program.ProductHeader;
            this.httpClient = httpClient;
        }

        public IObservable<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl)
        {
            var request = new Request
            {
                Method = HttpMethod.Get,
                BaseAddress = enterpriseBaseUrl,
                Endpoint = endPoint,
                Timeout = TimeSpan.FromSeconds(3),
                AllowAutoRedirect = false,
            };
            request.Headers.Add("User-Agent", productHeader.ToString());

            return httpClient.Send<object>(request, CancellationToken.None)
                .ToObservable()
                .Catch(Observable.Return<IResponse<object>>(null))
                .Select(resp => resp == null
                    ? EnterpriseProbeResult.Failed
                    : (resp.StatusCode == HttpStatusCode.OK
                        ? EnterpriseProbeResult.Ok
                        : EnterpriseProbeResult.NotFound));
        }

        public async Task<EnterpriseProbeResult> AsyncProbe(Uri enterpriseBaseUrl)
        {
            return await Probe(enterpriseBaseUrl).FirstAsync();
        }
    }

    public enum EnterpriseProbeResult
    {
        /// <summary>
        /// Yep! It's an Enterprise server
        /// </summary>
        Ok,

        /// <summary>
        /// Got a response from a server, but it wasn't an Enterprise server
        /// </summary>
        NotFound,

        /// <summary>
        /// Request timed out or DNS failed. So it's probably the case it's not an enterprise server but 
        /// we can't know for sure.
        /// </summary>
        Failed
    }
}
