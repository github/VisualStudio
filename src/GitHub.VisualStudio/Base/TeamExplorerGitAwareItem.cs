using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    /// <summary>
    /// This is a temporary base class that raises
    /// events with git repo information based on
    /// the solution open and close events. Once
    /// TFSContext is git-aware, this can be killed
    /// </summary>
    public class TeamExplorerGitAwareItem : TeamExplorerItemBase
    {
        bool disposed = false;

        protected void Initialize()
        {
            // temporary hack to update navigation item by tracking the solution
            SubscribeSolutionEvents();
        }

        /* Listen to solution events so we can use the solution
        to locate the github repo it's on and update navigation
        items accordingly. This is temporary until the
        TFS api supports git repos. */

        uint cookie = 0;
        void SubscribeSolutionEvents()
        {
            Debug.Assert(ServiceProvider != null, "ServiceProvider must be set before subscribing to solution events");
            if (cookie > 0)
                return;

            var solService = ServiceProvider.GetSolution();
            if (!ErrorHandler.Succeeded(solService.AdviseSolutionEvents(new SolutionEventListener(SolutionOpen), out cookie)))
            {
                Debug.Assert(false, "Unable to start listening for solution events");
            }
        }

        void UnsubscribeSolutionEvents()
        {
            Debug.Assert(ServiceProvider != null, "ServiceProvider must be set before subscribing to solution events");
            if (cookie == 0)
                return;

            var solService = ServiceProvider.GetSolution();
            if (!ErrorHandler.Succeeded(solService.UnadviseSolutionEvents(cookie)))
            {
                Debug.Assert(false, "Unable to stop listening for solution events");
            }
            cookie = 0;
        }

        void SolutionOpen()
        {
            var tc = new TeamContext();
            var solution = ServiceProvider.GetSolution();
            var repo = Services.GetRepoFromSolution(solution);
            if (repo == null)
                tc.HasTeamProject = false;
            else
            {
                tc.TeamProjectUri = Services.GetUriFromRepository(repo);
                if (tc.TeamProjectUri == null)
                {
                    tc.TeamProjectName = null;
                    tc.HasTeamProject = false;
                }
                else
                {
                    tc.TeamProjectName = tc.TeamProjectUri.GetUser() + "/" + tc.TeamProjectUri.GetRepo();
                    tc.HasTeamProject = true;
                }
            }

            ContextChanged(this, new ContextChangedEventArgs(CurrentContext, tc, false, true, false));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposed)
                return;

            if (disposing)
                UnsubscribeSolutionEvents();

            disposed = true;
        }

        class SolutionEventListener : IVsSolutionEvents
        {
            Action callback;
            public SolutionEventListener(Action callback)
            {
                this.callback = callback;
            }

            public int OnAfterCloseSolution(object pUnkReserved)
            {
                return 0;
            }

            public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
            {
                return 0;
            }

            public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
            {
                return 0;
            }

            public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
            {
                if (callback != null)
                    callback();
                return 0;
            }

            public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
            {
                return 0;
            }

            public int OnBeforeCloseSolution(object pUnkReserved)
            {
                return 0;
            }

            public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
            {
                return 0;
            }

            public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
            {
                return 0;
            }

            public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
            {
                return 0;
            }

            public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
            {
                return 0;
            }
        }

        class TeamContext : ITeamFoundationContext
        {
            public bool HasCollection { get; set; }
            public bool HasTeam { get; set; }
            public bool HasTeamProject { get; set; }
            public Guid TeamId { get; set; }
            public string TeamName { get; set; }
            public TfsTeamProjectCollection TeamProjectCollection { get; set; }
            [AllowNull]
            public string TeamProjectName { get; set; }
            [AllowNull]
            public Uri TeamProjectUri { get; set; }
        }
    }
}
