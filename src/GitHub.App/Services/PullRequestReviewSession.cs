using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHub.Models;

namespace GitHub.Services
{
    public class PullRequestReviewSession : IPullRequestReviewSession
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        Dictionary<string, List<IPullRequestReviewCommentModel>> comments;

        public PullRequestReviewSession(
            ILocalRepositoryModel repository,
            IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            Repository = repository;
            this.comments = comments.GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public ILocalRepositoryModel Repository { get; }

        public IEnumerable<IPullRequestReviewCommentModel> GetCommentsForFile(string filePath)
        {
            var relativePath = RootedPathToRelativePath(filePath).Replace('\\', '/');
            var result = Empty;
            comments.TryGetValue(relativePath, out result);
            return result ?? Empty;
        }

        string RootedPathToRelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                var repoRoot = Repository.LocalPath;

                if (path.StartsWith(repoRoot) && path.Length > repoRoot.Length + 1)
                {
                    return path.Substring(repoRoot.Length + 1);
                }
            }

            return null;
        }
    }
}
