using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using GitHub.Models;
using NullGuard;

namespace GitHub.Services
{
    [NullGuard(ValidationFlags.None)]
    public class PullRequestReviewSession : IPullRequestReviewSession, IDisposable
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        readonly Subject<Unit> changed;
        Dictionary<string, List<IPullRequestReviewCommentModel>> comments;

        public PullRequestReviewSession(
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel repository)
        {
            User = user;
            PullRequest = pullRequest;
            Repository = repository;
            changed = new Subject<Unit>();
            this.comments = pullRequest.Comments
                .OrderBy(x => x.Id)
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public IAccount User { get; }
        public IPullRequestModel PullRequest { get; }
        public ILocalRepositoryModel Repository { get; }
        public string CompareViewHackPath { get; private set; }
        public IObservable<Unit> Changed => changed;

        public void AddComment(IPullRequestReviewCommentModel comment)
        {
            List<IPullRequestReviewCommentModel> fileComments;

            if (!comments.TryGetValue(comment.Path, out fileComments))
            {
                fileComments = new List<IPullRequestReviewCommentModel>();
                comments.Add(comment.Path, fileComments);
            }

            fileComments.Add(comment);
            changed.OnNext(Unit.Default);
        }

        public IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path)
        {
            List<IPullRequestReviewCommentModel> result;
            path = path.Replace('\\', '/');
            return comments.TryGetValue(path, out result) ? result : Empty;
        }

        public IDisposable OpeningCompareViewHack(string path)
        {
            CompareViewHackPath = path;
            return Disposable.Create(() => CompareViewHackPath = null);
        }

        public void Dispose()
        {
            changed.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
