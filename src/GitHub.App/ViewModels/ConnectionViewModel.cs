using GitHub.Models;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public class ConnectionViewModel : BaseViewModel
    {
        protected IConnection Connection {[return: AllowNull] get; private set; }

        /// <summary>
        /// Host owning the repos
        /// </summary>
        public IRepositoryHost RepositoryHost { get; private set; }

        public ConnectionViewModel([AllowNull]IConnection connection, IRepositoryHosts hosts)
            : base(null)
        {
            if (connection != null)
            {
                Connection = connection;
                RepositoryHost = hosts.LookupHost(Connection.HostAddress);
            }
            else
                RepositoryHost = hosts.LookupHost(null);
        }
    }
}
