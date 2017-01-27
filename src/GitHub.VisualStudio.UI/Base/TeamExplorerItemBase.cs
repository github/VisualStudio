using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerItemBase : TeamExplorerGitRepoInfo, IServiceProviderAware
    {
        readonly ISimpleApiClientFactory apiFactory;
        protected ITeamExplorerServiceHolder holder;

        ISimpleApiClient simpleApiClient;
        [AllowNull]
        public ISimpleApiClient SimpleApiClient
        {
            [return: AllowNull] get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    apiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        protected ISimpleApiClientFactory ApiFactory => apiFactory;

        public TeamExplorerItemBase(IGitHubServiceProvider serviceProvider, ITeamExplorerServiceHolder holder)
            : base(serviceProvider)
        {
            this.holder = holder;
        }

        public TeamExplorerItemBase(IGitHubServiceProvider serviceProvider,
            ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
            : base(serviceProvider)
        {
            this.apiFactory = apiFactory;
            this.holder = holder;
        }

        public virtual void Initialize(IServiceProvider serviceProvider)
        {
#if DEBUG
            //VsOutputLogger.WriteLine("{0:HHmmssff}\t{1} Initialize", DateTime.Now, GetType());
#endif
            TEServiceProvider = serviceProvider;
            Debug.Assert(holder != null, "Could not get an instance of TeamExplorerServiceHolder");
            if (holder == null)
                return;
            holder.ServiceProvider = TEServiceProvider;
            SubscribeToRepoChanges();
#if DEBUG
            //VsOutputLogger.WriteLine("{0:HHmmssff}\t{1} Initialize DONE", DateTime.Now, GetType());
#endif
        }


        public virtual void Execute()
        {
        }

        public virtual void Invalidate()
        {
        }

        void SubscribeToRepoChanges()
        {
            holder.Subscribe(this, (ILocalRepositoryModel repo) =>
            {
                var changed = !Equals(ActiveRepo, repo);
                ActiveRepo = repo;
                RepoChanged(changed);
            });
        }

        void Unsubscribe()
        {
            holder.Unsubscribe(this);
            if (TEServiceProvider != null)
                holder.ClearServiceProvider(TEServiceProvider);
        }

        protected virtual void RepoChanged(bool changed)
        {
            var repo = ActiveRepo;
            if (repo != null)
            {
                var uri = repo.CloneUrl;
                if (uri?.RepositoryName != null)
                {
                    ActiveRepoUri = uri;
                    ActiveRepoName = uri.NameWithOwner;
                }
            }
        }

        protected async Task<RepositoryOrigin> GetRepositoryOrigin()
        {
            if (ActiveRepo == null)
                return RepositoryOrigin.NonGitRepository;

            var uri = ActiveRepoUri;
            if (uri == null)
                return RepositoryOrigin.Other;

            Debug.Assert(apiFactory != null, "apiFactory cannot be null. Did you call the right constructor?");
            SimpleApiClient = await apiFactory.Create(uri);

            var isdotcom = HostAddress.IsGitHubDotComUri(uri.ToRepositoryUrl());

            if (isdotcom)
            {
                return RepositoryOrigin.DotCom;
            }
            else
            {
                var repo = await SimpleApiClient.GetRepository();

                if ((repo.FullName == ActiveRepoName || repo.Id == 0) && SimpleApiClient.IsEnterprise())
                {
                    return RepositoryOrigin.Enterprise;
                }
            }

            return RepositoryOrigin.Other;
        }

        protected async Task<bool> IsAGitHubRepo()
        {
            var origin = await GetRepositoryOrigin();
            return origin == RepositoryOrigin.DotCom || origin == RepositoryOrigin.Enterprise;
        }

        protected async Task<bool> IsUserAuthenticated()
        {
            if (SimpleApiClient == null)
            {
                if (ActiveRepo == null)
                    return false;

                var uri = ActiveRepoUri;
                if (uri == null)
                    return false;

                SimpleApiClient = await apiFactory.Create(uri);
            }

            return SimpleApiClient?.IsAuthenticated() ?? false;
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    Unsubscribe();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }

        bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; this.RaisePropertyChange(); }
        }

        bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; this.RaisePropertyChange(); }
        }

        string text;
        [AllowNull]
        public string Text
        {
            get { return text; }
            set { text = value; this.RaisePropertyChange(); }
        }

    }
}