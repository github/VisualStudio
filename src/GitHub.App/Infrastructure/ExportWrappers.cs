using System.ComponentModel.Composition;
using Octokit.Internal;
using System;
using System.Net.Http;

namespace GitHub.Logging
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
}
