using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using NullGuard;
using System.Threading.Tasks;
using GitHub.Api;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        [ImportingConstructor]
        public OpenLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory)
            : base(serviceProvider, apiFactory)
        {
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.openLinkCommand;

        public async void Activate([AllowNull]object data = null)
        {
            var isgithub = await IsGitHubRepo();
            if (!isgithub)
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            var browser = ServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());
        }

        public bool CanShow()
        {
            return IsGitHubRepo().Wait(250);
        }
    }
}
