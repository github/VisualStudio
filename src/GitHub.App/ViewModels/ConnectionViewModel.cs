using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public class ConnectionViewModel : BaseViewModel
    {
        protected IConnection Connection { get; private set; }

        /// <summary>
        /// Host owning the repos
        /// </summary>
        public IRepositoryHost RepositoryHost { get; private set; }

        public ConnectionViewModel(IConnection connection, IRepositoryHosts hosts)
            : base(null)
        {
            Connection = connection;
            RepositoryHost = hosts.LookupHost(Connection.HostAddress);
        }
    }
}
