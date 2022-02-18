using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews
{
    public static class PullRequestFilterPackageGuids
    {
        public const string GuidPullRequestFilterPackageCmdSetString = "7cde2dfc-43c9-41ff-bf2e-bef41cd99e09";
        public const int PullRequestFilterId = 0x0100;
    }

    [SolutionTreeFilterProvider(PullRequestFilterPackageGuids.GuidPullRequestFilterPackageCmdSetString, PullRequestFilterPackageGuids.PullRequestFilterId)]
    [Export]
    public class PullRequestFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly IVsHierarchyItemCollectionProvider hierarchyCollectionProvider;
        private readonly IGitHubServiceProvider githubServiceProvider;

        [ImportingConstructor]
        public PullRequestFilterProvider(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider, IGitHubServiceProvider githubServiceProvider)
        {
            this.hierarchyCollectionProvider = hierarchyCollectionProvider;
            this.githubServiceProvider = githubServiceProvider;
        }

        protected override HierarchyTreeFilter CreateFilter()
        {
            return new PullRequestFilter(hierarchyCollectionProvider, githubServiceProvider);
        }

        private sealed class PullRequestFilter : HierarchyTreeFilter
        {
            private readonly IVsHierarchyItemCollectionProvider hierarchyCollectionProvider;
            private readonly IGitHubServiceProvider githubServiceProvider;
            private IPullRequestSessionManager sessionManager;
            private HashSet<string> pullRequestSessionFiles;

            public PullRequestFilter(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider, IGitHubServiceProvider githubServiceProvider)
            {
                this.hierarchyCollectionProvider = hierarchyCollectionProvider;
                this.githubServiceProvider = githubServiceProvider;
            }

            IPullRequestSessionManager SessionManager
            {
                get
                {
                    // Lazily load the pull request session manager to prevent all of our assemblies
                    // being loaded on VS startup.
                    if (sessionManager == null)
                    {
                        sessionManager = githubServiceProvider.GetService<IPullRequestSessionManager>();
                    }

                    return sessionManager;
                }
            }

            // Gets the items to be included from this filter provider.
            // rootItems is a collection that contains the root of your solution
            // Returns a collection of items to be included as part of the filter
            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                var root = HierarchyUtilities.FindCommonAncestor(rootItems);
                var sourceItems = await hierarchyCollectionProvider.GetDescendantsAsync(root.HierarchyIdentity.NestedHierarchy, CancellationToken);

                var vsSolution = githubServiceProvider.GetSolution();
                string solutionDirectory;
                string _;
                vsSolution.GetSolutionInfo(out solutionDirectory, out _, out _);

                this.pullRequestSessionFiles = new HashSet<string>();
                if (SessionManager.CurrentSession != null)
                {
                    var requestSessionFiles = await SessionManager.CurrentSession.GetAllFiles();
                    requestSessionFiles.ForEach(file => this.pullRequestSessionFiles.Add(BuildAbsolutePath(solutionDirectory, file.RelativePath)));
                }

                return await hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, ShouldIncludeInFilter, CancellationToken);
            }

            // Returns true if filters hierarchy item name for given filter; otherwise, false</returns>
            private bool ShouldIncludeInFilter(IVsHierarchyItem hierarchyItem)
            {
                return hierarchyItem?.CanonicalName != null && pullRequestSessionFiles.Contains(hierarchyItem.CanonicalName.ToUpperInvariant());
            }

            private static string BuildAbsolutePath(string solutionDirectory, string fileRelativePath)
            {
                return Path.Combine(solutionDirectory, fileRelativePath.Replace("/", @"\")).ToUpperInvariant();
            }
        }
    }
}
