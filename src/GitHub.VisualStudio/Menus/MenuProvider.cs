using GitHub.Exports;
using GitHub.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Menus
{
    /// <summary>
    /// This is a thin MEF wrapper around the MenuProvider
    /// which is registered as a global VS service. This class just
    /// redirects every request to the actual service, and can be
    /// thrown away as soon as the caller is done (no state is kept)
    /// </summary>
    [ExportForProcess(typeof(IMenuProvider), "devenv")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MenuProviderDispatcher : IMenuProvider
    {
        readonly IMenuProvider theRealProvider;

        [ImportingConstructor]
        public MenuProviderDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            theRealProvider = serviceProvider.GetService(typeof(IMenuProvider)) as IMenuProvider;
        }

        public IReadOnlyCollection<IDynamicMenuHandler> DynamicMenus => theRealProvider.DynamicMenus;

        public IReadOnlyCollection<IMenuHandler> Menus => theRealProvider.Menus;
    }

    internal class MenuProvider : IMenuProvider
    {
        public IReadOnlyCollection<IMenuHandler> Menus { get; }

        public IReadOnlyCollection<IDynamicMenuHandler> DynamicMenus { get; }

        public MenuProvider(IGitHubServiceProvider serviceProvider)
        {
            Menus = new List<IMenuHandler>
            {
                new AddConnection(serviceProvider),
                new OpenPullRequests(serviceProvider),
                new ShowGitHubPane(serviceProvider)
            };

            DynamicMenus = new List<IDynamicMenuHandler>
            {
                new CopyLink(serviceProvider),
                new CreateGist(serviceProvider),
                new OpenLink(serviceProvider),
                new BlameLink(serviceProvider)
            };
        }
    }
}
