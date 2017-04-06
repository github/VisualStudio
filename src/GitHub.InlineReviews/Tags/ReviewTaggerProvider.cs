using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Tags
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(ReviewTag))]
    class ReviewTaggerProvider : ITaggerProvider
    {
        readonly IPullRequestReviewSessionManager sessionManager;

        [ImportingConstructor]
        public ReviewTaggerProvider(IPullRequestReviewSessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new ReviewTagger(buffer, sessionManager) as ITagger<T>;
        }
    }
}
