using GitHub.Primitives;
using System.Collections.ObjectModel;

namespace GitHub.Models
{
    public interface IConnectionManager
    {
        IConnection CreateConnection(HostAddress address, string username);
        bool AddConnection(HostAddress address, string username);
        bool RemoveConnection(HostAddress address);
        ObservableCollection<IConnection> Connections { get; }
    }
}
