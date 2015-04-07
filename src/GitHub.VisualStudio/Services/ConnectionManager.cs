using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GitHub.Models;
using GitHub.Services;
using GitHub.Primitives;

namespace GitHub.VisualStudio
{
    class CacheData
    {
        public IEnumerable<ConnectionCacheItem> connections;
    }

    class ConnectionCacheItem
    {
        public Uri HostUrl { get; set; }
        public string UserName { get; set; }
    }

    [Export(typeof(IConnectionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConnectionManager : IConnectionManager
    {
        readonly string cachePath;
        const string cacheFile = "ghfvs.connections";

        public event Action<IConnection> RequiresLogin;
        public IObservable<IConnection> LoginComplete { get; set; }

        [ImportingConstructor]
        public ConnectionManager(IProgram program)
        {
            Connections = new ObservableCollection<IConnection>();
            cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                program.ApplicationName,
                cacheFile);

            LoadConnectionsFromCache();

            Connections.CollectionChanged += RefreshConnections;
        }

        public IConnection CreateConnection(HostAddress address, string username)
        {
            return new Connection(this, address, username);
        }

        public bool AddConnection(HostAddress address, string username)
        {
            if (Connections.FirstOrDefault(x => x.HostAddress == address) != null)
                return false;
            Connections.Add(new Connection(this, address, username));
            return true;
        }

        public bool RemoveConnection(HostAddress address)
        {
            var c = Connections.FirstOrDefault(x => x.HostAddress == address);
            if (c == null)
                return false;
            Connections.Remove(c);
            return true;
        }

        public IObservable<IConnection> RequestLogin(IConnection connection)
        {
            if (LoginComplete == null)
                return null;
            var handler = RequiresLogin;
            if (handler == null)
                return null;
            handler(connection);
            return LoginComplete;
        }

        void RefreshConnections(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveConnectionsToCache();
        }

        void LoadConnectionsFromCache()
        {
            EnsureCachePath();

            if (!File.Exists(cachePath))
                return;

            string data = File.ReadAllText(cachePath, Encoding.UTF8);

            CacheData cacheData;
            try
            {
                cacheData = SimpleJson.DeserializeObject<CacheData>(data);
            }
            catch
            {
                cacheData = null;
            }

            if (cacheData == null || cacheData.connections == null)
            {
                // cache is corrupt, remove
                File.Delete(cachePath);
                return;
            }

            cacheData.connections.ForEach(c =>
            {
                if (c.HostUrl != null)
                    AddConnection(HostAddress.Create(c.HostUrl), c.UserName);
            });
        }

        void SaveConnectionsToCache()
        {
            EnsureCachePath();

            var cache = new CacheData();
            cache.connections = Connections.Select(conn =>
                new ConnectionCacheItem { HostUrl = conn.HostAddress.WebUri, UserName = conn.Username });
            try
            {
                string data = SimpleJson.SerializeObject(cache);
                File.WriteAllText(cachePath, data);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        void EnsureCachePath()
        {
            if (File.Exists(cachePath))
                return;
            var di = Path.GetDirectoryName(cachePath);
            if (!Directory.Exists(di))
                Directory.CreateDirectory(di);
        }

        public ObservableCollection<IConnection> Connections { get; private set; }
    }
}
