using System.ComponentModel.Composition;
using GitHub.Models;
using Octokit;
using Octokit.Internal;

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
