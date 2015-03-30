using GitHub.Primitives;
using System.Collections.ObjectModel;

namespace GitHub.Models
{
    public interface IConnectionManager
    {
        IConnection CreateConnection(HostAddress address, string username);
        ObservableCollection<IConnection> Connections { get; }
    }
}
