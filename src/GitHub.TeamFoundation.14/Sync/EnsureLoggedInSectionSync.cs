using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Services;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    // TODO: The IsAGitHubRepo() is somehow giving false positives, need to fix that
    // before reactivating this, it's annoying users.
    //[TeamExplorerSection(SyncLoginSectionId, TeamExplorerPageIds.GitCommits, 10)]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class EnsureLoggedInSectionSync : EnsureLoggedInSection
    {
        public const string SyncLoginSectionId = "C5975729-3CF1-47B4-AE92-C2934906CDDA";

        [ImportingConstructor]
        public EnsureLoggedInSectionSync(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, ITeamExplorerServices teServices)
            : base(serviceProvider, apiFactory, holder, cm, teServices)
        {}
    }
}