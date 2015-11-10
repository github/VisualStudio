using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio.Helpers;
using NullGuard;
using Octokit;
using GitHub.Extensions;

namespace GitHub.VisualStudio.Base
{
    public class TeamExplorerItemBase : TeamExplorerGitRepoInfo
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

        public TeamExplorerItemBase(ITeamExplorerServiceHolder holder)
        {
            this.holder = holder;
        }

        public TeamExplorerItemBase(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder)
        {
            this.apiFactory = apiFactory;
            this.holder = holder;
        }

        public virtual void Execute()
        {
        }

        public virtual void Invalidate()
        {
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

        protected async Task<bool> IsAGitHubRepo()
        {
            var uri = ActiveRepoUri;
            if (uri == null)
                return false;

            SimpleApiClient = apiFactory.Create(uri);

            var isdotcom = HostAddress.IsGitHubDotComUri(uri.ToRepositoryUrl());
            if (!isdotcom)
            {
                var repo = await SimpleApiClient.GetRepository();
                return (repo.FullName == ActiveRepoName || repo.Id == 0) && SimpleApiClient.IsEnterprise();
            }
            return isdotcom;
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

    [Export(typeof(IGitHubClient))]
    public class GHClient : GitHubClient
    {
        [ImportingConstructor]
        public GHClient(IProgram program)
            : base(program.ProductHeader)
        {

        }
    }
}