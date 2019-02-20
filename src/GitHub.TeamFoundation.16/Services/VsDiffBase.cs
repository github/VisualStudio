using System;
using System.ComponentModel.Composition;
using System.Threading;
using GitHub.Services;
using Microsoft.VisualStudio.RichReview.Contracts;
using Microsoft.VisualStudio.RichReview.UX.Controls;
using Microsoft.VisualStudio.RichReview.UX.Controls.ViewModel;
using Microsoft.VisualStudio.Shell;

namespace GitHub.TeamFoundation.Services
{
    internal class VsDiffBaseFactory
    {
        [ImportingConstructor]
        public VsDiffBaseFactory(
            [Import("Microsoft.VisualStudio.RichReview.Contracts.IDiffBaseService", AllowDefault = true)] Lazy<object> diffBaseServiceObject)
        {
            if (diffBaseServiceObject != null)
            {
                Instance = new VsDiffBase(diffBaseServiceObject);
            }
        }

        [Export(typeof(IVsDiffBase))]
        internal IVsDiffBase Instance { get; }
    }

    internal class VsDiffBase : IVsDiffBase
    {
        readonly Lazy<object> diffBaseServiceObject;

        // store this in a static because this class does not seem to be a singleton
        static object ChangesList;

        internal VsDiffBase(Lazy<object> diffBaseServiceObject)
        {
            this.diffBaseServiceObject = diffBaseServiceObject;
        }

        public void SetDiffBase(string repoPath, string branchName)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (diffBaseServiceObject.Value is IDiffBaseService diffBaseService)
                {
                    try
                    {
                        string mergeBase = await diffBaseService.DiffBaseInformationProvider.GetMergeBaseAsync(repoPath, branchName, CancellationToken.None);
                        diffBaseService.DiffBaseInfo = new DiffBaseInfo() { Id = mergeBase, ShortDescription = branchName, LongDescription = branchName };
                    }
                    catch
                    {
                        // currently hitting errors if the target branch does not exist - will have to fetch from the proper remote. How to be sure what remote?
                        // for prototyping, let's just eat all exceptions and clear the DiffBase if we can't figure it out.
                        diffBaseService.DiffBaseInfo = null;
                    }
                }
            });
        }

        public object GetChangesList()
        {
            if (ChangesList == null)
            {
                ChangesList = new ChangesSectionList() { DataContext = new ChangesNewPRViewModel() };
            }
            return ChangesList;
        }
    }
}
