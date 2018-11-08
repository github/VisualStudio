using System;
using System.Reactive;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public sealed class PullRequestCheckViewModelDesigner : ViewModelBase, IPullRequestCheckViewModel
    {
        public string Title { get; set; } = "continuous-integration/appveyor/pr";

        public string Description { get; set; } = "AppVeyor build failed";

        public PullRequestCheckStatus Status { get; set; } = PullRequestCheckStatus.Failure;

        public Uri DetailsUrl { get; set; } = new Uri("http://github.com");

        public ReactiveCommand<Unit, Unit> OpenDetailsUrl { get; set; } = null;

        public PullRequestCheckType CheckType { get; set; } = PullRequestCheckType.ChecksApi;

        public string CheckRunId { get; set; }

        public bool HasAnnotations { get; } = true;
    }
}