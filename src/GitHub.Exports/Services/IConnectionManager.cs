using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
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
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event Func<IConnection, IObservable<IConnection>> DoLogin;
        Task RefreshRepositories();
    }
}
