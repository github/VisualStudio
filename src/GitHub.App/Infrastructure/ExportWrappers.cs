using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
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
    public class ExportedHttpListener : IHttpListener
    {
        readonly IHttpListener inner = new HttpListenerWrapper(new HttpListener());
        public AuthenticationSchemes AuthenticationSchemes { get { return inner.AuthenticationSchemes; } set { inner.AuthenticationSchemes = value; } }
        public AuthenticationSchemeSelector AuthenticationSchemeSelectorDelegate { get { return inner.AuthenticationSchemeSelectorDelegate; } set { inner.AuthenticationSchemeSelectorDelegate = value; } }
        public ServiceNameCollection DefaultServiceNames => inner.DefaultServiceNames;
        public ExtendedProtectionPolicy ExtendedProtectionPolicy { get { return inner.ExtendedProtectionPolicy; } set { inner.ExtendedProtectionPolicy = value; } }
        public HttpListener.ExtendedProtectionSelector ExtendedProtectionSelectorDelegate { get { return inner.ExtendedProtectionSelectorDelegate; } set { inner.ExtendedProtectionSelectorDelegate = value; } }
        public bool IgnoreWriteExceptions { get { return inner.IgnoreWriteExceptions; } set { inner.IgnoreWriteExceptions = value; } }
        public bool IsListening => inner.IsListening;
        public ICollection<string> Prefixes => inner.Prefixes;
        public string Realm { get { return inner.Realm; } set { inner.Realm = value; } }
        public HttpListenerTimeoutManager TimeoutManager => inner.TimeoutManager;
        public bool UnsafeConnectionNtlmAuthentication { get { return inner.UnsafeConnectionNtlmAuthentication; } set { inner.UnsafeConnectionNtlmAuthentication = value; } }
        public void Abort() => inner.Abort();
        public IAsyncResult BeginGetContext(AsyncCallback callback, object state) => inner.BeginGetContext(callback, state);
        public void Close() => inner.Close();
        public IHttpListenerContext EndGetContext(IAsyncResult asyncResult) => inner.EndGetContext(asyncResult);
        public IHttpListenerContext GetContext() => inner.GetContext();
        public Task<IHttpListenerContext> GetContextAsync() => inner.GetContextAsync();
        public void Start() => inner.Start();
        public void Stop() => inner.Stop();
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
