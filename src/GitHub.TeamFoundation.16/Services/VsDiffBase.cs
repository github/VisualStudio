using System;
using System.ComponentModel.Composition;
using System.Threading;
using GitHub.Logging;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.DiffBase.Contracts;
using Microsoft.VisualStudio.DiffBase.Contracts.Git;
using Microsoft.VisualStudio.DiffBase.Controls;
using Microsoft.VisualStudio.DiffBase.Controls.ViewModel;
using Microsoft.VisualStudio.Shell;
using Serilog;

namespace GitHub.TeamFoundation.Services
{
    internal class VsDiffBaseFactory
    {
        [ImportingConstructor]
        public VsDiffBaseFactory(
            [Import("Microsoft.VisualStudio.DiffBase.Contracts.IDiffBaseService", AllowDefault = true)] Lazy<object> diffBaseServiceObject,
            IGitClient gitClient,
            IGitService gitService,
            IGitHubServiceProvider serviceProvider)
        {
            if (diffBaseServiceObject != null)
            {
                Instance = new VsDiffBase(diffBaseServiceObject, gitClient, gitService, serviceProvider);
            }
        }

        [Export(typeof(IVsDiffBase))]
        internal IVsDiffBase Instance { get; }
    }

    internal class VsDiffBase : IVsDiffBase
    {
        static readonly ILogger log = LogManager.ForContext<VsDiffBase>();
        readonly Lazy<object> diffBaseServiceObject;
        readonly IGitClient gitClient;
        readonly IGitService gitService;
        readonly IGitHubServiceProvider serviceProvider;

        // store this in a static because this class does not seem to be a singleton
        static object ChangesList;

        internal VsDiffBase(Lazy<object> diffBaseServiceObject, IGitClient gitClient, IGitService gitService, IGitHubServiceProvider serviceProvider)
        {
            this.diffBaseServiceObject = diffBaseServiceObject;
            this.gitClient = gitClient;
            this.gitService = gitService;
            this.serviceProvider = serviceProvider;
        }

        public void SetDiffBase(string repoPath, UriString cloneRepo, string branchName)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    if (diffBaseServiceObject.Value is IDiffBaseService diffBaseService)
                    {
                        var diffBaseContext = diffBaseService.GetGlobalContext();

                        try
                        {
                            using (var repo = this.gitService.GetRepository(repoPath))
                            {
                                // TODO: Is the fetch necessary? Maybe, maybe not.
                                await this.gitClient.Fetch(repo, cloneRepo, branchName);

                                // Get the ref to the remote branch
                                string branchRef = branchName;

                                if (branchRef != null)
                                {
                                    foreach (var remote in repo.Network.Remotes)
                                    {
                                        if (UriString.RepositoryUrlsAreEqual(new UriString(remote.Url), cloneRepo))
                                        {
                                            branchRef = $"refs/remotes/{remote.Name}/{branchRef}";
                                            break;
                                        }
                                    }
                                }

                                // get the merge base to that branch and use that as the diffBase
                                string mergeBase = await ((IDiffBaseInformationProviderGit)diffBaseContext.DiffBaseInformationProvider).GetMergeBaseAsync(repoPath, branchRef, CancellationToken.None);
                                diffBaseContext.DiffBaseInfo = new DiffBaseInfo() { Id = mergeBase, ShortDescription = branchName, LongDescription = branchName };
                            }
                        }
                        catch
                        {
                            diffBaseContext.DiffBaseInfo = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, nameof(SetDiffBase));
                }
            });
        }

        public object GetChangesList()
        {
            if (ChangesList == null)
            {
                try
                {
                    if (diffBaseServiceObject.Value is IDiffBaseService diffBaseService)
                    {
                        var diffBaseContext = diffBaseService.GetGlobalContext();

                        var viewModel = new ChangedFileListViewModel() { DiffBaseContext = diffBaseContext };
                        ChangesList = new ChangedFileList() { DataContext = viewModel };
                        viewModel.Initialize(this.serviceProvider, null);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, nameof(GetChangesList));
                }
            }
            return ChangesList;
        }

        private class DiffBaseInfo : IDiffBaseInfo
        {
            public string Id { get; set; }
            public string ShortDescription { get; set; }
            public string LongDescription { get; set; }
        }
    }
}
