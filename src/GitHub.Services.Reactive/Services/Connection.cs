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
            Repositories = new ObservableCollection<ILocalRepositoryModel>();
        }

        public HostAddress HostAddress { get; private set; }
        public string Username { get; private set; }
        public ObservableCollection<ILocalRepositoryModel> Repositories { get; }

        public IObservable<IConnection> Login()
        {
            return manager.RequestLogin(this);
        }

        public void Logout()
        {
            manager.RequestLogout(this);
        }

        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Repositories.Clear();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
