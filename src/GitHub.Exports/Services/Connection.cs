using System;
using GitHub.Models;
using GitHub.Primitives;

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
        }

        public HostAddress HostAddress { get; private set; }
        public string Username { get; private set; }

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
