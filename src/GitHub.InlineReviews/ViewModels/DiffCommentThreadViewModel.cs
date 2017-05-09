using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class DiffCommentThreadViewModel : ReactiveObject, IDiffCommentThreadViewModel
    {
        public DiffCommentThreadViewModel(
            string diffHunk,
            string path,
            InlineCommentThreadViewModel comments)
        {
            DiffHunk = LastLines(diffHunk);
            Path = path;
            Comments = comments;
        }

        public string DiffHunk { get; }
        public string Path { get; }
        public ICommentThreadViewModel Comments { get; }

        string LastLines(string diffHunk)
        {
            var lines = new List<string>();

            using (var reader = new StringReader(diffHunk))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            var result = new StringBuilder();

            foreach (var line in lines.Skip(Math.Max(0, lines.Count - 5)))
            {
                result.AppendLine(line);
            }

            return result.ToString();
        }
    }
}
