using System;
using System.Diagnostics.CodeAnalysis;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestDetailViewModelDesigner : BaseViewModel, IPullRequestDetailViewModel
    {
        public IAccount Author { get; set; }
        public string Body { get; set; }
        public int Number { get; set; }

        public ReactiveCommand<object> OpenOnGitHub { get; }
    }
}