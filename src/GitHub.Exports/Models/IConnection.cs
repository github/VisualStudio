using GitHub.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace GitHub.Models
{
    public interface IConnection
    {
        HostAddress HostAddress { get; }
        string Username { get; }
        IObservable<IConnection> Login();
        void Logout();
    }
}
