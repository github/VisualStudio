using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
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

        Func<string, bool> fileExists;
        Func<string, Encoding, string> readAllText;
        Action<string, string> writeAllText;
        Action<string> fileDelete;
        Func<string, bool> dirExists;
        Action<string> dirCreate;

        [ImportingConstructor]
        public ConnectionManager(IProgram program)
        {
            fileExists = (path) => System.IO.File.Exists(path);
            readAllText = (path, encoding) => System.IO.File.ReadAllText(path, encoding);
            writeAllText = (path, content) => System.IO.File.WriteAllText(path, content);
            fileDelete = (path) => System.IO.File.Delete(path);
            dirExists = (path) => System.IO.Directory.Exists(path);
            dirCreate = (path) => System.IO.Directory.CreateDirectory(path);

            Connections = new ObservableCollection<IConnection>();
            cachePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                program.ApplicationName,
                cacheFile);

            LoadConnectionsFromCache();

            Connections.CollectionChanged += RefreshConnections;
        }

        public ConnectionManager(IProgram program, Rothko.IOperatingSystem os)
        {
            fileExists = (path) => os.File.Exists(path);
            readAllText = (path, encoding) => os.File.ReadAllText(path, encoding);
            writeAllText = (path, content) => os.File.WriteAllText(path, content);
            fileDelete = (path) => os.File.Delete(path);
            dirExists = (path) => os.Directory.Exists(path);
            dirCreate = (path) => os.Directory.CreateDirectory(path);

            Connections = new ObservableCollection<IConnection>();
            cachePath = System.IO.Path.Combine(
                os.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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

            if (!fileExists(cachePath))
                return;

            string data = readAllText(cachePath, Encoding.UTF8);

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
                fileDelete(cachePath);
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
                writeAllText(cachePath, data);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
        }

        void EnsureCachePath()
        {
            if (fileExists(cachePath))
                return;
            var di = System.IO.Path.GetDirectoryName(cachePath);
            if (!dirExists(di))
                dirCreate(di);
        }

        public ObservableCollection<IConnection> Connections { get; private set; }
    }
}
