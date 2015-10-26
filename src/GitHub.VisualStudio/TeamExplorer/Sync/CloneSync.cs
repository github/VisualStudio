using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.VisualStudio.Base;
using Microsoft.TeamFoundation.Controls;
using GitHub.Api;
using GitHub.VisualStudio.Helpers;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.Models;
using GitHub.Extensions;
using System.Reactive.Linq;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerNavigationItem(CloneSyncId, NavigationItemPriority.Graphs, TargetPageId=TeamExplorerPageIds.GitCommits)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CloneSync : TeamExplorerNavigationItemBase
    {
        public const string CloneSyncId = "92655B52-360D-4BF5-95C5-D9E9E596AC77";
        readonly Lazy<ICloneSyncViewModel> vm;

        [ImportingConstructor]
        public CloneSync(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            Lazy<ICloneSyncViewModel> vm)
            : base(apiFactory, holder, Octicon.repo_clone)
        {
            Text = Resources.SynchronizeCloneText;
            ArgbColor = Colors.LightBlueNavigationItem.ToInt32();
            this.vm = vm;
        }

        public async override void Execute()
        {
            await Task.Run(() =>
            {
                var obs = vm.Value.Sync(ActiveRepo);
                obs.Subscribe();
                return true;
            });
            base.Execute();
        }
    }
}
