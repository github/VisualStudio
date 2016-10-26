using GitHub.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace GitHub.VisualStudio.Menus
{
    public class MenuProvider : IMenuProvider
    {
        public IReadOnlyCollection<IMenuHandler> Menus { get; }

        public IReadOnlyCollection<IDynamicMenuHandler> DynamicMenus { get; }

        public MenuProvider(IUIProvider serviceProvider)
        {
            Menus = new List<IMenuHandler>
            {
                new AddConnection(serviceProvider),
                new OpenPullRequests(),
                new ShowGitHubPane()
            };

            DynamicMenus = new List<IDynamicMenuHandler>
            {
                new CopyLink(serviceProvider),
                new CreateGist(serviceProvider),
                new OpenLink(serviceProvider)
            };
        }
    }
}
