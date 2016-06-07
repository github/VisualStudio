using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using NullGuard;

namespace GitHub.VisualStudio.Menus
{
    [Export(typeof(IDynamicMenuHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenLink: LinkMenuBase, IDynamicMenuHandler
    {
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public OpenLink([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider, IUsageTracker usageTracker)
            : base(serviceProvider)
        {
            this.usageTracker = usageTracker;
        }

        public Guid Guid => GuidList.guidContextMenuSet;
        public int CmdId => PkgCmdIDList.openLinkCommand;

        public void Activate([AllowNull]object data = null)
        {
            if (!IsGitHubRepo())
                return;

            var link = GenerateLink();
            if (link == null)
                return;
            var browser = ServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            browser?.OpenUrl(link.ToUri());

            usageTracker.IncrementOpenInGitHubCount();
        }

        public bool CanShow()
        {
            return IsGitHubRepo();
        }
    }
}
