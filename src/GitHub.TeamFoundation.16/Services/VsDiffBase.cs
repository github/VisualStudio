using System.ComponentModel.Composition;
using System.Threading;
using GitHub.Services;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.RichReview.Contracts;
using Microsoft.VisualStudio.RichReview.UX.Controls;
using Microsoft.VisualStudio.RichReview.UX.Controls.ViewModel;
using Microsoft.VisualStudio.Shell;

namespace GitHub.TeamFoundation.Services
{
    [Export(typeof(IVsDiffBase))]
    internal class VsDiffBase : IVsDiffBase
    {
        // store this in a static because this class does not seem to be a singleton
        private static object ChangesList;

        public void SetDiffBase(string repoPath, string branchName)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // Note: Asking for this up front in the constructor causes MEF cardinality issues much of the time.
                // Delaying when I ask for it seems to have better results.
                var componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
                var diffBaseService = componentModel.GetService<IDiffBaseService>();

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
