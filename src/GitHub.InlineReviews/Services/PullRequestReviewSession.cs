using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Services
{
    public class PullRequestReviewSession : IPullRequestReviewSession, IDisposable
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        readonly Subject<Unit> changed;
        readonly Dictionary<string, List<IPullRequestReviewCommentModel>> pullRequestComments;
        readonly Dictionary<string, List<IInlineCommentModel>> inlineComments;

        public PullRequestReviewSession(
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel repository)
        {
            User = user;
            PullRequest = pullRequest;
            Repository = repository;
            changed = new Subject<Unit>();
            pullRequestComments = pullRequest.ReviewComments
                .OrderBy(x => x.Id)
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public IAccount User { get; }
        public IPullRequestModel PullRequest { get; }
        public ILocalRepositoryModel Repository { get; }
        public IObservable<Unit> Changed => changed;
        public IObservable<IReadOnlyList<IInlineCommentModel>> InlineComments { get; }

        public void AddComment(IPullRequestReviewCommentModel comment)
        {
            List<IPullRequestReviewCommentModel> fileComments;

            if (!pullRequestComments.TryGetValue(comment.Path, out fileComments))
            {
                fileComments = new List<IPullRequestReviewCommentModel>();
                pullRequestComments.Add(comment.Path, fileComments);
            }

            fileComments.Add(comment);
            changed.OnNext(Unit.Default);
        }

        public IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path)
        {
            List<IPullRequestReviewCommentModel> result;
            path = path.Replace('\\', '/');
            return pullRequestComments.TryGetValue(path, out result) ? result : Empty;
        }

        public void Dispose()
        {
            changed.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
