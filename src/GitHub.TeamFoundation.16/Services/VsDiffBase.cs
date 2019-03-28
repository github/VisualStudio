using System;
using System.ComponentModel.Composition;
using System.Threading;
using GitHub.Logging;
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
            IGitHubServiceProvider serviceProvider)
        {
            if (diffBaseServiceObject != null)
            {
                Instance = new VsDiffBase(diffBaseServiceObject, serviceProvider);
            }
        }

        [Export(typeof(IVsDiffBase))]
        internal IVsDiffBase Instance { get; }
    }

    internal class VsDiffBase : IVsDiffBase
    {
        static readonly ILogger log = LogManager.ForContext<VsDiffBase>();
        readonly Lazy<object> diffBaseServiceObject;
        readonly IGitHubServiceProvider serviceProvider;

        // store this in a static because this class does not seem to be a singleton
        static object ChangesList;

        internal VsDiffBase(Lazy<object> diffBaseServiceObject, IGitHubServiceProvider serviceProvider)
        {
            this.diffBaseServiceObject = diffBaseServiceObject;
            this.serviceProvider = serviceProvider;
        }

        public void SetDiffBase(string repoPath, string branchName)
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
                            string mergeBase = await ((IDiffBaseInformationProviderGit)diffBaseContext.DiffBaseInformationProvider).GetMergeBaseAsync(repoPath, branchName, CancellationToken.None);
                            diffBaseContext.DiffBaseInfo = new DiffBaseInfo() { Id = mergeBase, ShortDescription = branchName, LongDescription = branchName };
                        }
                        catch
                        {
                            // currently hitting errors if the target branch does not exist - will have to fetch from the proper remote. How to be sure what remote?
                            // for prototyping, let's just eat all exceptions and clear the DiffBase if we can't figure it out.
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
