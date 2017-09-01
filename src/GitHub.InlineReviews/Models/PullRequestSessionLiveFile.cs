using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using GitHub.Models;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Models
{
    public class PullRequestSessionLiveFile : PullRequestSessionFile, IDisposable
    {
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

        public void Dispose()
        {
            ToDispose?.Dispose();
            ToDispose = null;
        }
    }
}
