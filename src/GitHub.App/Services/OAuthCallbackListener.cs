using System.ComponentModel.Composition;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GitHub.Api;
using GitHub.Extensions;
using Rothko;
using static System.FormattableString;

namespace GitHub.Services
{
    /// <summary>
    /// Listens for a callback from the OAuth endpoint on "http://localhost:42549".
    /// </summary>
    /// <remarks>
    /// The GitHub for Visual Studio OAUTH application on GitHub is configured to call back to
    /// http://localhost:42549 on successful login. This class listens on that port and returns
    /// the temporary code received from the callback to the caller.
    /// 
    /// Note that as implemented this class can only listen for one OAUTH callback at a time. If
    /// <see cref="Listen"/> is called when already listening for a callback, the original listen
    /// operation will throw an <see cref="OperationCanceledException"/>.
    /// </remarks>
    [Export(typeof(IOAuthCallbackListener))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OAuthCallbackListener : IOAuthCallbackListener
    {
        const int CallbackPort = 42549;
        readonly static string CallbackUrl = Invariant($"http://localhost:{CallbackPort}/");
        readonly IHttpListener httpListener;

        [ImportingConstructor]
        public OAuthCallbackListener(IHttpListener httpListener)
        {
            Guard.ArgumentNotNull(httpListener, nameof(httpListener));

            this.httpListener = httpListener;
            httpListener.Prefixes.Add(CallbackUrl);
        }

        public async Task<string> Listen(string id, CancellationToken cancel)
        {
            var internalCancel = new CancellationTokenSource();
            var combinedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, internalCancel.Token);

            if (httpListener.IsListening) httpListener.Stop();
            httpListener.Start();

            try
            {
                var complete = new TaskCompletionSource<HttpListenerContext>();

                using (cancel.Register(httpListener.Stop))
                {
                    var context = await httpListener.GetContextAsync();
                    var foo = context.Request;
                    var queryParts = HttpUtility.ParseQueryString(context.Request.Url.Query);

                    if (queryParts["state"] == id)
                    {
                        context.Response.Close();
                        return queryParts["code"];
                    }
                }

                throw new WebException("The login returned an unexpected response.");
            }
            finally
            {
                httpListener.Stop();
            }
        }
    }
}
