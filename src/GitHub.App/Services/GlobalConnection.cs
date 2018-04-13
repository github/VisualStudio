using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.App.Services
{
    [Export(typeof(IGlobalConnection))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GlobalConnection : IGlobalConnection
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public GlobalConnection(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IConnection Get() => serviceProvider.TryGetMEFComponent<IConnection>();
    }
}
