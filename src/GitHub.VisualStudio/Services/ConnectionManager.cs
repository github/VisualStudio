using GitHub.Models;
using GitHub.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConnectionManager
    {
        public ObservableCollection<IConnection> Connections { get; private set; }

        public ConnectionManager()
        {
            Connections = new ObservableCollection<IConnection>();

            // TODO: Load list of known connections from cache
            Connections.CollectionChanged += RefreshConnections;
        }

        private void RefreshConnections(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // TODO: save list of known connections to cache
        }
    }
}
