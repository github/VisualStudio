using System;
using System.IO;
using GitHub.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GitHub.VisualStudio
{
    class CacheData
    {
        public IEnumerable<IConnection> connections;
    }

    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConnectionManager
    {
        readonly string cachePath;
        const string cacheFile = "ghfvs.connections";
        public ObservableCollection<IConnection> Connections { get; private set; }

        public ConnectionManager(IProgram program)
        {
            Connections = new ObservableCollection<IConnection>();
            cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), program.ApplicationProvider, cacheFile);

            LoadConnectionsFromCache();

            // TODO: Load list of known connections from cache
            Connections.CollectionChanged += RefreshConnections;
        }

        void RefreshConnections(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // TODO: save list of known connections to cache
            SaveConnectionsToCache();
        }

        void LoadConnectionsFromCache()
        {
            if (!File.Exists(cacheFile))
                return;
            string data = File.ReadAllText(cacheFile);

            CacheData cacheData;
            try
            {
                cacheData = SimpleJson.DeserializeObject<CacheData>(data);
            }
            catch
            {
                // cache is corrupt, remove
                File.Delete(cacheFile);
                return;
            }
            foreach (var c in cacheData.connections)
                Connections.Add(c);
        }

        void SaveConnectionsToCache()
        {
            CacheData cache = new CacheData();
            cache.connections = Connections.ToList();
            try
            {
                string data = SimpleJson.SerializeObject(cache);
                File.WriteAllText(cachePath, data);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
            }
        }

    }
}
