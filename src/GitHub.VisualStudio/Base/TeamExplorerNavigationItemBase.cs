using Microsoft.TeamFoundation.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using NullGuard;
using GitHub.VisualStudio.Base;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.TeamFoundation.Client;
using GitHub.Api;

namespace GitHub.VisualStudio
{
    class TeamExplorerNavigationItemBase : TeamExplorerItemBase, ITeamExplorerNavigationItem2, INotifyPropertySource
    {
        protected readonly ISimpleApiClientFactory apiFactory;
        public ISimpleApiClient SimpleApiClient { get; private set;  }

        public TeamExplorerNavigationItemBase(IServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory)
            : base()
        {
            this.ServiceProvider = serviceProvider;
            this.apiFactory = apiFactory;

            // temporary hack to update navigation item by tracking the solution
            SubscribeSolutionEvents();
        }

        // temporary hack to update navigation item by tracking the solution
        public override void Dispose()
        {
            UnsubscribeSolutionEvents();
            base.Dispose();
        }

        int argbColor;
        public int ArgbColor
        {
            get { return argbColor; }
            set { argbColor = value; this.RaisePropertyChange(); }
        }

        object icon;
        [AllowNull]
        public object Icon
        {
            get { return icon; }
            set { icon = value; this.RaisePropertyChange(); }
        }

        Image image;
        [AllowNull]
        public Image Image
        {
            get { return image; }
            set { image = value; this.RaisePropertyChange(); }
        }

        protected virtual async void UpdateState()
        {
            bool visible = false;

            if (SimpleApiClient == null)
            {
                var solution = ServiceProvider.GetSolution();
                var uri = Services.GetRepoUrlFromSolution(solution);
                if (uri == null)
                {
                    IsVisible = false;
                    return;
                }

                if (HostAddress.IsGitHubDotComUri(uri))
                    visible = true;

                SimpleApiClient = apiFactory.Create(uri);

                if (!visible)
                {
                    // enterprise probe
                    var ret = await SimpleApiClient.IsEnterprise();
                    visible = (ret == GitHub.Services.EnterpriseProbeResult.Ok);
                }

                IsVisible = visible;
            }
        }



        /* Listen to solution events so we can use the solution
        to locate the github repo it's on and update navigation
        items accordingly. This is temporary until the
        TFS api supports git repos. */

        uint cookie = 0;
        void SubscribeSolutionEvents()
        {
            if (cookie > 0)
                return;
            var solService = ServiceProvider.GetSolution();
            solService.AdviseSolutionEvents(new SolutionEventListener(SolutionOpen), out cookie);
        }

        void UnsubscribeSolutionEvents()
        {
            if (cookie == 0)
                return;
            var solService = ServiceProvider.GetSolution();
            solService.UnadviseSolutionEvents(cookie);
            cookie = 0;
        }

        void SolutionOpen()
        {
            ContextChanged(this, new ContextChangedEventArgs(CurrentContext, CurrentContext, false, true, false));
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
    }
}
