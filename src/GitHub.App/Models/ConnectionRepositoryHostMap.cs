using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;
using GitHub.Extensions;

namespace GitHub.ViewModels
{
    [Export(typeof(IConnectionRepositoryHostMap))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionRepositoryHostMap : IConnectionRepositoryHostMap
    {
        [ImportingConstructor]
        public ConnectionRepositoryHostMap(IGitHubServiceProvider provider, IRepositoryHosts hosts)
            : this(provider.TryGetService<IConnection>(), hosts)
        {
        }

        public ConnectionRepositoryHostMap(IRepositoryHost repositoryHost)
        {
            CurrentRepositoryHost = repositoryHost;
        }

        protected ConnectionRepositoryHostMap(IConnection connection, IRepositoryHosts hosts)
            : this(hosts.LookupHost(connection.HostAddress))
        {
        }

        /// <summary>
        /// The current repository host. This is set in the MEF sub-container when the user clicks on an action
        /// related to a host such as clone or create.
        /// </summary>
        public IRepositoryHost CurrentRepositoryHost { get; private set; }
    }
}
