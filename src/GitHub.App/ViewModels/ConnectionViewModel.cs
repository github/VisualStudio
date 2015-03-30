using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public class ConnectionViewModel : ReactiveObject
    {
        protected IConnection Connection { get; private set; }

        /// <summary>
        /// Host owning the repos
        /// </summary>
        public IRepositoryHost RepositoryHost { get; private set; }

        public ConnectionViewModel(IConnection connection, IRepositoryHosts hosts)
        {
            Connection = connection;
            RepositoryHost = hosts.LookupHost(Connection.HostAddress);
        }
    }
}
