/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio;
using Octokit;

namespace Microsoft.TeamExplorerSample.Sync
{
    [Export]
    public class HomeSectionViewModel: INotifyPropertyChanged
    {
        readonly ISimpleApiClientFactory apiFactory;

        string repoUrl;
        string repoName;
        ISimpleApiClient simpleApiClient;
        Octicon icon;

        [ImportingConstructor]
        public HomeSectionViewModel(CompositionServices compositionServices)
        {
            var exportProvider = compositionServices.GetExportProvider();
            var teamExplorerContext = exportProvider.GetExportedValue<ITeamExplorerContext>();
            teamExplorerContext.PropertyChanged += TeamExplorerContextOnPropertyChanged;

            apiFactory = exportProvider.GetExportedValue<ISimpleApiClientFactory>();
        }

        void TeamExplorerContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var teamExplorerContext = (ITeamExplorerContext) sender;
            if (e.PropertyName == nameof(ITeamExplorerContext.ActiveRepository))
            {
                var cloneUrl = teamExplorerContext.ActiveRepository?.CloneUrl;
                var repositoryUri = cloneUrl?.ToRepositoryUrl();
                RepoUrl = repositoryUri?.ToString();
                RepoName = cloneUrl?.NameWithOwner;
                Icon = GetIcon();

                if (RepoUrl != null)
                {
                    UpdateRepositoryIconAsync().Forget();
                }
            }
        }

        async Task UpdateRepositoryIconAsync()
        {
            if (RepoUrl == null)
                return;

            if (simpleApiClient != null)
            {
                apiFactory.ClearFromCache(simpleApiClient);
                simpleApiClient = null;
            }

            simpleApiClient = await apiFactory.Create(new UriString(repoUrl));
            var repository = await simpleApiClient.GetRepository();
            Icon = GetIcon(repository);
        }

        public Octicon Icon
        {
            get { return icon; }
            set
            {
                if (icon != value)
                { 
                    icon = value;
                    OnPropertyChanged(nameof(RepoUrl));
                }
            }
        }

        public string RepoUrl
        {
            get { return repoUrl; }
            set
            {
                if (repoUrl != value)
                {
                    repoUrl = value;
                    OnPropertyChanged(nameof(RepoUrl));
                }
            }
        }

        public string RepoName
        {
            get { return repoName; }
            set
            {
                if (repoName != value)
                {
                    repoName = value;
                    OnPropertyChanged(nameof(RepoName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        static Octicon GetIcon(Repository repo = null)
        {
            var isPrivate = repo?.Private ?? false;
            var isFork = repo?.Fork ?? false;

            return isPrivate ? Octicon.@lock
                : isFork ? Octicon.repo_forked : Octicon.repo;
        }
    }
}
