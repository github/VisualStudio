using System;
using System.Collections.Generic;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.Models
{
    class PullRequestSessionFile : ReactiveObject, IPullRequestSessionFile
    {
        IList<DiffChunk> diff;
        string relativePath;

        public string RelativePath
        {
            get { return relativePath; }
            internal set { this.RaiseAndSetIfChanged(ref relativePath, value); }
        }

        public IList<DiffChunk> Diff
        {
            get { return diff; }
            internal set { this.RaiseAndSetIfChanged(ref diff, value); }
        }

        public string BaseCommit { get; internal set; }
        public string BaseSha { get; internal set; }

        public IReactiveList<IInlineCommentThreadModel> InlineCommentThreads { get; }
            = new ReactiveList<IInlineCommentThreadModel>();
    }
}
