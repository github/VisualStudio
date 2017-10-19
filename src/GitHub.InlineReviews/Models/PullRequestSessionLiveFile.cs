using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using GitHub.Models;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Models
{
    /// <summary>
    /// A file in a pull request session that tracks editor content.
    /// </summary>
    /// <remarks>
    /// A live session file extends <see cref="PullRequestSessionFile"/> to update the file's
    /// review comments in real time, based on the contents of an editor and
    /// <see cref="IPullRequestSessionManager.CurrentSession"/>.
    /// </remarks>
    public sealed class PullRequestSessionLiveFile : PullRequestSessionFile, IDisposable
    {
        public PullRequestSessionLiveFile(
            string relativePath,
            ITextBuffer textBuffer,
            ISubject<ITextSnapshot, ITextSnapshot> rebuild)
            : base(relativePath)
        {
            TextBuffer = textBuffer;
            Rebuild = rebuild;
        }

        /// <summary>
        /// Gets the VS text buffer that the file is associated with.
        /// </summary>
        public ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Gets a resource to dispose.
        /// </summary>
        public IDisposable ToDispose { get; internal set; }

        /// <summary>
        /// Gets a dictionary mapping review comments to tracking points in the <see cref="TextBuffer"/>.
        /// </summary>
        public IDictionary<IInlineCommentThreadModel, ITrackingPoint> TrackingPoints { get; internal set; }

        /// <summary>
        /// Gets an observable raised when the review comments for the file should be rebuilt.
        /// </summary>
        public ISubject<ITextSnapshot, ITextSnapshot> Rebuild { get; }

        /// <summary>
        /// Disposes of the resources in <see cref="ToDispose"/>.
        /// </summary>
        public void Dispose()
        {
            ToDispose?.Dispose();
            ToDispose = null;
        }
    }
}
