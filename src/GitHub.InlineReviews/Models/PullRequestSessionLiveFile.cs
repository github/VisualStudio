using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using GitHub.Models;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Models
{
    public class PullRequestSessionLiveFile : PullRequestSessionFile, IPullRequestSessionLiveFile, IDisposable
    {
        readonly Subject<IList<int>> linesChanged = new Subject<IList<int>>();

        public PullRequestSessionLiveFile(
            string relativePath,
            ITextView textView,
            ISubject<ITextSnapshot, ITextSnapshot> rebuild)
            : base(relativePath)
        {
            TextView = textView;
            Rebuild = rebuild;
        }

        public ITextView TextView { get; }
        public IDisposable ToDispose { get; internal set; }
        public IDictionary<IInlineCommentThreadModel, ITrackingPoint> TrackingPoints { get; internal set; }
        public ISubject<ITextSnapshot, ITextSnapshot> Rebuild { get; }
        public IObservable<IList<int>> LinesChanged => linesChanged;

        public override IReadOnlyList<IInlineCommentThreadModel> InlineCommentThreads
        {
            get { return base.InlineCommentThreads; }
            internal set
            {
                var lines = base.InlineCommentThreads?
                    .Concat(value ?? Enumerable.Empty<IInlineCommentThreadModel>())
                    .Select(x => x.LineNumber)
                    .Where(x => x >= 0)
                    .Distinct()
                    .ToList();
                base.InlineCommentThreads = value;
                NotifyLinesChanged(lines);
            }
        }

        public void Dispose()
        {
            ToDispose?.Dispose();
            ToDispose = null;
        }

        public void NotifyLinesChanged(IList<int> lines) => linesChanged.OnNext(lines);
    }
}
