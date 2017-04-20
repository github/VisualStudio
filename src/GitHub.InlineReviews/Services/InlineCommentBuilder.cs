using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IInlineCommentBuilder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InlineCommentBuilder : IInlineCommentBuilder
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;

        [ImportingConstructor]
        public InlineCommentBuilder(IGitService gitService, IGitClient gitClient)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));

            this.gitService = gitService;
            this.gitClient = gitClient;
        }

        public async Task<IList<InlineCommentModel>> Build(
            string path,
            ITextSnapshot snapshot,
            IPullRequestReviewSession session)
        {
            Guard.ArgumentNotNull(path, nameof(path));
            Guard.ArgumentNotNull(session, nameof(session));

            var comments = session.GetCommentsForFile(path);
            var result = new List<InlineCommentModel>();

            if (comments.Count == 0)
                return result;

            var commentsByCommit = comments
                .Where(x => x.OriginalPosition.HasValue)
                .OrderBy(x => x.OriginalPosition)
                .ThenBy(x => x.Id)
                .GroupBy(x => x.OriginalCommitId);
            var repo = gitService.GetRepository(session.Repository.LocalPath);
            var current = snapshot.GetText();
            var differ = new InlineDiffBuilder(new Differ());

            foreach (var commit in commentsByCommit)
            {
                var baseSha = session.PullRequest.Base.Sha;
                var @base = await gitClient.ExtractFile(repo, baseSha, path) ?? string.Empty;
                var snapshotDiff = BuildDiff(differ.BuildDiffModel(@base, current));
                var inlineComments = CreateInlineComments(commit);

                foreach (var comment in inlineComments)
                {
                    var match = snapshotDiff.IndexOf(comment.DiffHunk);

                    if (match != -1)
                    {
                        comment.LineNumber = LineFromPosition(snapshotDiff, match) + comment.DiffHunkLines - 1;
                        result.Add(comment);
                    }
                }
            }

            return result;
        }

        static string BuildDiff(DiffPaneModel diffModel)
        {
            var builder = new StringBuilder();

            foreach (var line in diffModel.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        builder.Append('+');
                        break;
                    case ChangeType.Deleted:
                        builder.Append('-');
                        break;
                    default:
                        builder.Append(' ');
                        break;
                }

                builder.AppendLine(line.Text);
            }

            return builder.ToString();
        }

        static IList<InlineCommentModel> CreateInlineComments(IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            return comments.Select(x => new InlineCommentModel(x)).ToList();
        }

        static int LineFromPosition(string s, int position)
        {
            var result = 0;

            for (var i = 0; i < position; ++i)
            {
                if (s[i] == '\n') ++result;
            }

            return result;
        }
    }
}
