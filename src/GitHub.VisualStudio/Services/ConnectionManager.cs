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
using Rothko;
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
        readonly IOperatingSystem operatingSystem;
        const string cacheFile = "ghfvs.connections";

        [ImportingConstructor]
        public ConnectionManager(IProgram program, IOperatingSystem operatingSystem)
        {
            this.operatingSystem = operatingSystem;

            Connections = new ObservableCollection<IConnection>();
            cachePath = Path.Combine(
                operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                program.ApplicationProvider,
                cacheFile);

            LoadConnectionsFromCache();

            Connections.CollectionChanged += RefreshConnections;
        }

        public IConnection CreateConnection(HostAddress address, string username)
        {
            return new Connection(address, username);
        }

        public bool AddConnection(HostAddress address, string username)
        {
            if (Connections.FirstOrDefault(x => x.HostAddress == address) != null)
                return false;
            Connections.Add(new Connection(address, username));
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

        void RefreshConnections(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveConnectionsToCache();
        }

        void LoadConnectionsFromCache()
        {
            if (!operatingSystem.File.Exists(cachePath))
                return;

            string data = operatingSystem.File.ReadAllText(cachePath, Encoding.UTF8);

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
                operatingSystem.File.Delete(cachePath);
                return;
            }

            cacheData.connections.All(c =>
            {
                if (c.HostUrl != null)
                    AddConnection(HostAddress.Create(c.HostUrl), c.UserName);
                return true;
            });
        }

        void SaveConnectionsToCache()
        {
            var cache = new CacheData();
            cache.connections = Connections.Select(conn =>
                new ConnectionCacheItem { HostUrl = conn.HostAddress.WebUri, UserName = conn.Username });
            try
            {
                string data = SimpleJson.SerializeObject(cache);
                operatingSystem.File.WriteAllText(cachePath, data);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        public ObservableCollection<IConnection> Connections { get; private set; }
    }
}
