using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        readonly IDiffService diffService;
        readonly IPullRequestReviewSession session;
        readonly IRepository repository;
        readonly string path;
        readonly bool leftHandSide;
        readonly string tabsToSpaces;
        readonly IReadOnlyList<IPullRequestReviewCommentModel> comments;
        Dictionary<int, List<DiffLine>> diffHunks;
        string baseCommit;

        public InlineCommentBuilder(
            IGitClient gitClient,
            IDiffService diffService,
            IPullRequestReviewSession session,
            IRepository repository,
            string path,
            bool leftHandSide,
            int? tabsToSpaces)
        {
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(path, nameof(path));

            this.gitClient = gitClient;
            this.diffService = diffService;
            this.session = session;
            this.repository = repository;
            this.path = path;
            this.leftHandSide = leftHandSide;
            
            if (tabsToSpaces.HasValue)
            {
                this.tabsToSpaces = new string(' ', tabsToSpaces.Value);
            }

            comments = session.GetCommentsForFile(path);
        }

        public async Task<IList<InlineCommentModel>> BuildComments(ITextSnapshot snapshot)
        {
            Guard.ArgumentNotNull(snapshot, nameof(snapshot));

            if (diffHunks == null) BuildDiffHunks();
            if (baseCommit == null) await ExtractBaseCommit();

            return await Task.Run(() =>
            {
                var current = snapshot.GetText();
                var snapshotDiff = diffService.Diff(baseCommit, current, 4).ToList();
                var result = new List<InlineCommentModel>();

                foreach (var comment in comments)
                {
                    var hunk = diffHunks[comment.Id];
                    var match = Match(snapshotDiff, hunk);
                    var lineNumber = GetLineNumber(match);

                    if (lineNumber != -1)
                    {
                        var snapshotLine = snapshot.GetLineFromLineNumber(lineNumber);
                        var trackingPoint = snapshot.CreateTrackingPoint(snapshotLine.Start, PointTrackingMode.Positive);
                        result.Add(new InlineCommentModel(lineNumber, comment, trackingPoint));
                    }
                }

                return result;
            });
        }

        public async Task<IList<int>> GetAddCommentLines(ITextSnapshot snapshot)
        {
            Guard.ArgumentNotNull(snapshot, nameof(snapshot));

            var result = new List<int>();

            if (!await gitClient.IsModified(repository, path, snapshot.GetText()))
            {
                var patch = await gitClient.Compare(
                    repository,
                    session.PullRequest.Base.Sha,
                    repository.Head.Tip.Sha,
                    path);
                var diff = diffService.ParseFragment(patch.Content);

                foreach (var chunk in diff)
                {
                    foreach (var line in chunk.Lines)
                    {
                        if (line.NewLineNumber != -1)
                            result.Add(line.NewLineNumber);
                    }
                }
            }

            return result;
        }

        void BuildDiffHunks()
        {
            diffHunks = new Dictionary<int, List<DiffLine>>();

            foreach (var comment in comments)
            {
                var last = diffService.ParseFragment(comment.DiffHunk).Last();
                diffHunks.Add(comment.Id, last.Lines.Reverse().Take(5).ToList());
            }
        }

        DiffLine Match(IEnumerable<DiffChunk> diff, List<DiffLine> target)
        {
            int j = 0;

            foreach (var source in diff)
            {
                for (var i = source.Lines.Count - 1; i >= 0; --i)
                {
                    if (source.Lines[i].Content == target[j].Content)
                    {
                        if (++j == target.Count) return source.Lines[i + j - 1];
                    }
                    else
                    {
                        j = 0;
                    }
                }
            }

            return null;
        }

        string TabsToSpaces(string s)
        {
            return tabsToSpaces != null ? s.Replace("\t", tabsToSpaces) : s;
        }

        async Task ExtractBaseCommit()
        {
            baseCommit = await gitClient.ExtractFile(
                repository,
                session.PullRequest.Base.Sha,
                path) ?? string.Empty;
        }

        int GetLineNumber(DiffLine line)
        {
            if (line != null)
            {
                if (leftHandSide && line.OldLineNumber != -1)
                    return line.OldLineNumber - 1;
                if (!leftHandSide && line.NewLineNumber != -1)
                    return line.NewLineNumber - 1;
            }

            return -1;
        }
    }
}
