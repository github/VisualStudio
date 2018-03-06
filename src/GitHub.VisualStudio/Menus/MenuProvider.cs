using GitHub.Services;
using System.Collections.Generic;

namespace GitHub.VisualStudio.Menus
{
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
                new ShowGitHubPane(serviceProvider),
                new ShowCurrentPullRequest(serviceProvider)
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
