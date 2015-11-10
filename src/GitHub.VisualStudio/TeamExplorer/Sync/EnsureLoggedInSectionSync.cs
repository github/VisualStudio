using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamExplorer.Sync
{
    [TeamExplorerSection(SyncLoginSectionId, TeamExplorerPageIds.GitCommits, 10)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class EnsureLoggedInSectionSync : EnsureLoggedInSection
    {
        public const string SyncLoginSectionId = "C5975729-3CF1-47B4-AE92-C2934906CDDA";

        [ImportingConstructor]
        public EnsureLoggedInSectionSync(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder,
            IConnectionManager cm, IRepositoryHosts hosts, IVSServices vsServices)
            : base(apiFactory, holder, cm, hosts, vsServices)
        {}
    }
}