using GitHub.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Services;

namespace GitHub.Models
{
    public interface IConnectionManager
    {
        IConnection CreateConnection(HostAddress address, string username);
        bool AddConnection(HostAddress address, string username);
        bool RemoveConnection(HostAddress address);
        ObservableCollection<IConnection> Connections { get; }

        IObservable<IConnection> RequestLogin(IConnection connection);
        void RequestLogout(IConnection connection);

        // for telling IRepositoryHosts that we need to login from cache
        event Func<IConnection, IObservable<IConnection>> DoLogin;
    }
}
