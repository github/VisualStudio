using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitHub.Models;

namespace GitHub.InlineReviews.Models
{
    public class InlineCommentModel
    {
        public InlineCommentModel(IPullRequestReviewCommentModel original)
        {
            LineNumber = original.OriginalPosition.Value - 1;
            Original = original;

            var diffInfo = CalculateDiffHunk(original.DiffHunk);
            DiffHunk = diffInfo.Item1;
            DiffHunkLines = diffInfo.Item2;
        }

        public InlineCommentModel(int lineNumber, IPullRequestReviewCommentModel original)
        {
            LineNumber = lineNumber;
            Original = original;
        }

        public int LineNumber { get; set; }
        public string DiffHunk { get; }
        public int DiffHunkLines { get; }
        public IPullRequestReviewCommentModel Original { get; }
        public bool IsMoved => Original.Position != LineNumber + 1;

        public void UpdatePosition(int editLine, int editDelta)
        {
            if (LineNumber >= editLine)
            {
                LineNumber += editDelta;
            }
        }

        static Tuple<string, int> CalculateDiffHunk(string diff)
        {
            // This can definitely be done more efficiently!
            var lines = ReadLines(diff)
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

            return Tuple.Create(builder.ToString(), count);
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
    }
}
