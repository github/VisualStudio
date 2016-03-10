using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Collections;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestListViewModelDesigner : BaseViewModelDesigner, IPullRequestListViewModel
    {
        public PullRequestListViewModelDesigner()
        {
            var prs = new TrackingCollection<IPullRequestModel>(Observable.Empty<IPullRequestModel>());
            prs.Add(new PullRequestModel(399, "Let's try doing this differently", new AccountDesigner { Login = "shana", IsUser = true }, DateTimeOffset.Now - TimeSpan.FromDays(1)));
            prs.Add(new PullRequestModel(389, "Build system upgrade", new AccountDesigner { Login = "haacked", IsUser = true }, DateTimeOffset.Now - TimeSpan.FromMinutes(2)) { CommentCount = 4, HasNewComments = false });
            prs.Add(new PullRequestModel(409, "Fix publish button style", new AccountDesigner { Login = "shana", IsUser = false }, DateTimeOffset.Now - TimeSpan.FromHours(5)) { CommentCount = 27, HasNewComments = true });
            PullRequests = prs;
        }

        public ITrackingCollection<IPullRequestModel> PullRequests { get; set; }
        public IPullRequestModel SelectedPullRequest { get; set; }
        public ICommand OpenPullRequest { get; set; }
    }
}