using System;
using GitHub.Models;
using GitHub.SampleData;

namespace GitHub.InlineReviews.SampleData
{
    public class PullRequestReviewCommentDesigner : IPullRequestReviewCommentModel
    {
        public int Id => 1;
        public string Path => string.Empty;
        public int? Position => 1;
        public string CommitId => null;
        public string DiffHunk => null;
        public DateTimeOffset UpdatedAt => DateTime.Now.Subtract(TimeSpan.FromDays(3));
        public IAccount User => new AccountDesigner { Login = "shana", IsUser = true };
        public string Body => @"You can use a `CompositeDisposable` type here, it's designed to handle disposables in an optimal way (you can just call `Dispose()` on it and it will handle disposing everything it holds).";
    }
}
