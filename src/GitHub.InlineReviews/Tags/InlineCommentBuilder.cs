using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Services
{
    public class InlineCommentBuilder
    {
        readonly IGitClient gitClient;
        readonly IPullRequestReviewSession session;
        readonly string path;
        readonly IRepository repository;
        readonly IReadOnlyList<IPullRequestReviewCommentModel> comments;
        readonly InlineDiffBuilder differ = new InlineDiffBuilder(new Differ());
        Dictionary<int, DiffHunk> diffHunks;
        string baseCommit;

        public InlineCommentBuilder(
            IGitClient gitClient,
            IPullRequestReviewSession session,
            IRepository repository,
            string path)
        {
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(path, nameof(path));

            this.gitClient = gitClient;
            this.session = session;
            this.repository = repository;
            this.path = path;

            comments = session.GetCommentsForFile(path);
        }

        public async Task<IList<InlineCommentModel>> Update(ITextSnapshot snapshot)
        {
            Guard.ArgumentNotNull(snapshot, nameof(snapshot));

            if (diffHunks == null) BuildDiffHunks();
            if (baseCommit == null) await ExtractBaseCommit();

            return await Task.Run(() =>
            {
                var current = snapshot.GetText();
                var snapshotDiff = BuildDiff(differ.BuildDiffModel(baseCommit, current));
                var result = new List<InlineCommentModel>();

                foreach (var comment in comments)
                {
                    var hunk = diffHunks[comment.Id];
                    var match = snapshotDiff.IndexOf(hunk.Text);

                    if (match != -1)
                    {
                        var lineNumber = LineFromPosition(snapshotDiff, match) + hunk.LineCount - 1;
                        var snapshotLine = snapshot.GetLineFromLineNumber(lineNumber);
                        var trackingPoint = snapshot.CreateTrackingPoint(snapshotLine.Start, PointTrackingMode.Positive);
                        result.Add(new InlineCommentModel(lineNumber, comment, trackingPoint));
                    }
                }

                return result;
            });
        }

        void BuildDiffHunks()
        {
            diffHunks = new Dictionary<int, DiffHunk>();

            foreach (var comment in comments)
            {
                // This can definitely be done more efficiently!
                var lines = ReadLines(comment.DiffHunk)
                    .Reverse()
                    .Take(5)
                    .TakeWhile(x => !x.StartsWith("@@"))
                    .Reverse();
                var builder = new StringBuilder();
                var count = 0;

                foreach (var line in lines)
                {
                    builder.AppendLine(line);
                    ++count;
                }

                var hunk = new DiffHunk(builder.ToString(), count);
                diffHunks.Add(comment.Id, hunk);
            }
        }

        async Task ExtractBaseCommit()
        {
            baseCommit = await gitClient.ExtractFile(
                repository,
                session.PullRequest.Base.Sha,
                path) ?? string.Empty;
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

        static int LineFromPosition(string s, int position)
        {
            var result = 0;

            for (var i = 0; i < position; ++i)
            {
                if (s[i] == '\n') ++result;
            }

            return result;
        }

        static IEnumerable<string> ReadLines(string s)
        {
            using (var reader = new StringReader(s))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        class DiffHunk
        {
            public DiffHunk(string text, int lineCount)
            {
                Text = text;
                LineCount = lineCount;
            }

            public string Text { get; }
            public int LineCount { get; }
        }
    }
}
