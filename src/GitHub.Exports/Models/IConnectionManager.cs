using GitHub.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GitHub.Models
{
    public interface IConnectionManager
    {
        IConnection CreateConnection(HostAddress address, string username);
        bool AddConnection(HostAddress address, string username);
        bool RemoveConnection(HostAddress address);
        ObservableCollection<IConnection> Connections { get; }

        IObservable<IConnection> RequestLogin(IConnection connection);

        // for telling IRepositoryHosts that we need to login from cache
        event Action<IConnection> RequiresLogin;

        // registered by IRepositoryHosts to return an observable
        IObservable<IConnection> LoginComplete { get; set; }
    }
}
