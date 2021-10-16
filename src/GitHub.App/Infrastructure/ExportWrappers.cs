using System;
using System.ComponentModel.Composition;
using System.Net;
using GitHub.Models;
using Octokit;
using Octokit.Internal;
using Rothko;

namespace GitHub.Infrastructure
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

    [Export(typeof(IHttpListener))]
    public class ExportedHttpListener : HttpListenerWrapper
    {
        public ExportedHttpListener()
            : base(new HttpListener())
        {
        }
    }

    [Export(typeof(IEnterpriseProbe))]
    public class ExportedEnterpriseProbe : EnterpriseProbe
    {
        [ImportingConstructor]
        public ExportedEnterpriseProbe(IProgram program, IHttpClient client)
            : base(program.ProductHeader, client)
        {
        }
    }
}
