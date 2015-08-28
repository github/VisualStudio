using System;
using GitHub.Models;
using GitHub.Primitives;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GitHub.Services
{
    public class Connection : IConnection
    {
        readonly IConnectionManager manager;

        public Connection(IConnectionManager cm, HostAddress hostAddress, string userName)
        {
            manager = cm;
            HostAddress = hostAddress;
            Username = userName;
            Repositories = new ObservableCollection<ISimpleRepositoryModel>();
        }

        public HostAddress HostAddress { get; private set; }
        public string Username { get; private set; }
        public ObservableCollection<ISimpleRepositoryModel> Repositories { get; }

        public IObservable<IConnection> Login()
        {
            return manager.RequestLogin(this);
        }

        public void Logout()
        {
            manager.RequestLogout(this);
        }
    }
}
