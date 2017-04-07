using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IInlineCommentBuilder))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class InlineCommentBuilder : IInlineCommentBuilder
    {
        readonly IGitClient gitClient;

        [ImportingConstructor]
        public InlineCommentBuilder(IGitClient gitClient)
        {
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));

            this.gitClient = gitClient;
        }

        public async Task<IList<InlineCommentModel>> Build(
            string path,
            IPullRequestReviewSession session)
        {
            Guard.ArgumentNotNull(path, nameof(path));
            Guard.ArgumentNotNull(session, nameof(session));

            var result = new List<InlineCommentModel>();
            var repo = GitService.GitServiceHelper.GetRepository(session.Repository.LocalPath);
            var changes = await gitClient.Compare(
                repo,
                session.PullRequest.Head.Sha,
                session.PullRequest.Base.Sha,
                path);
            var comments = session.GetCommentsForFile(path);
            var diffPositions = comments
                .Where(x => x.Position.HasValue)
                .Select(x => x.Position.Value)
                .OrderBy(x => x)
                .Distinct();
            var lineMap = MapDiffPositions(changes.Content, diffPositions);

            foreach (var comment in comments)
            {
                int lineNumber;

                if (comment.Position.HasValue && lineMap.TryGetValue(comment.Position.Value, out lineNumber))
                {
                    result.Add(new InlineCommentModel(lineNumber, comment));
                }
            }

            return result;
        }

        /// <summary>
        /// Maps lines in a diff to lines in the source file.
        /// </summary>
        /// <param name="diff">The diff.</param>
        /// <param name="positions">The diff lines to map.</param>
        /// <returns>
        /// A dictionary mapping 1-based diff line numbers to 0-based file line numbers.
        /// </returns>
        public IDictionary<int, int> MapDiffPositions(string diff, IEnumerable<int> positions)
        {
            Guard.ArgumentNotNull(diff, nameof(diff));
            Guard.ArgumentNotNull(positions, nameof(positions));

            var diffLine = -1;
            var sourceLine = -1;
            var positionEnumerator = positions.GetEnumerator();
            var result = new Dictionary<int, int>();

            positionEnumerator.MoveNext();

            using (var reader = new StringReader(diff))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("@@"))
                    {
                        if (sourceLine == -1) diffLine = 0;
                        sourceLine = ReadLineFromHunkHeader(line) - 1;
                    }

                    if (positionEnumerator.Current == diffLine)
                    {
                        result.Add(diffLine, sourceLine);
                        if (!positionEnumerator.MoveNext()) break;
                    }

                    if (diffLine >= 0)
                    {
                        ++diffLine;
                        if (!line.StartsWith('-')) ++sourceLine;
                    }
                }
            }

            return result;
        }

        int ReadLineFromHunkHeader(string line)
        {
            int plus = line.IndexOf('+');
            int comma = line.IndexOf(',', plus);
            return int.Parse(line.Substring(plus + 1, comma - (plus + 1)));
        }

        public class ChunkRange
        {
            public int DiffLine { get; set; }
            public int SourceLine { get; set; }
        }
    }
}
