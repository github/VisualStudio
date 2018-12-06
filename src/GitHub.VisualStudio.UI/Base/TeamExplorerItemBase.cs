using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI;
using GitHub.Extensions;
using System.ComponentModel;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerItemBase : TeamExplorerGitRepoInfo, IServiceProviderAware
    {
        readonly ITeamExplorerServiceHolder holder;
        readonly ISimpleApiClientFactory apiFactory;

        ISimpleApiClient simpleApiClient;
        public ISimpleApiClient SimpleApiClient
        {
            get { return simpleApiClient; }
            set
            {
                if (simpleApiClient != value && value == null)
                    apiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = value;
            }
        }

        public TeamExplorerItemBase(IGitHubServiceProvider serviceProvider, ISimpleApiClientFactory apiFactory,
            ITeamExplorerServiceHolder holder) : this(serviceProvider, holder)
        {
            Guard.ArgumentNotNull(apiFactory, nameof(apiFactory));

            this.apiFactory = apiFactory;
        }

        public TeamExplorerItemBase(IGitHubServiceProvider serviceProvider, ITeamExplorerServiceHolder holder)
            : base(serviceProvider)
        {
            Guard.ArgumentNotNull(holder, nameof(holder));

            this.holder = holder;
        }

        public virtual void Initialize(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            TEServiceProvider = serviceProvider;
            Debug.Assert(holder != null, "Could not get an instance of TeamExplorerServiceHolder");
            if (holder == null)
                return;
            holder.ServiceProvider = TEServiceProvider;
            SubscribeToRepoChanges();
        }

        public virtual void Execute()
        {
        }

        public virtual void Invalidate()
        {
        }

        bool subscribedToRepoChanges = false;
        protected void SubscribeToRepoChanges()
        {
            if (!subscribedToRepoChanges)
            {
                subscribedToRepoChanges = true;
                UpdateRepoOnMainThread(holder.TeamExplorerContext.ActiveRepository);
                holder.TeamExplorerContext.PropertyChanged += TeamExplorerContext_PropertyChanged;
            }
        }

        void TeamExplorerContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(holder.TeamExplorerContext.ActiveRepository))
            {
                UpdateRepoOnMainThread(holder.TeamExplorerContext.ActiveRepository);
            }
        }

        void UpdateRepoOnMainThread(LocalRepositoryModel repo)
        {
            holder.JoinableTaskFactory.RunAsync(async () =>
            {
                await holder.JoinableTaskFactory.SwitchToMainThreadAsync();
                UpdateRepo(repo);
            }).Task.Forget();
        }

        void Unsubscribe()
        {
            holder.TeamExplorerContext.PropertyChanged -= TeamExplorerContext_PropertyChanged;

            if (TEServiceProvider != null)
                holder.ClearServiceProvider(TEServiceProvider);
        }

        void UpdateRepo(LocalRepositoryModel repo)
        {
            ActiveRepo = repo;
            RepoChanged();
            Invalidate();
        }

        protected virtual void RepoChanged()
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

        protected async Task<RepositoryOrigin> GetRepositoryOrigin(UriString uri)
        {
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

                if ((repo.FullName == ActiveRepoName || repo.Id == 0) && await SimpleApiClient.IsEnterprise())
                {
                    return RepositoryOrigin.Enterprise;
                }
            }

            return RepositoryOrigin.Other;
        }

        protected async Task<bool> IsAGitHubRepo(UriString uri)
        {
            var origin = await GetRepositoryOrigin(uri);
            return origin == RepositoryOrigin.DotCom || origin == RepositoryOrigin.Enterprise;
        }

        protected async Task<bool> IsAGitHubDotComRepo(UriString uri)
        {
            var origin = await GetRepositoryOrigin(uri);
            return origin == RepositoryOrigin.DotCom;
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
        public string Text
        {
            get { return text; }
            set { text = value; this.RaisePropertyChange(); }
        }
    }
}