using System;
using System.ComponentModel.Composition;
using GitHub.InlineReviews.Services;
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
        readonly IInlineCommentBuilder builder;

        [ImportingConstructor]
        public ReviewTaggerProvider(
            IPullRequestReviewSessionManager sessionManager,
            IInlineCommentBuilder builder)
        {
            this.sessionManager = sessionManager;
            this.builder = builder;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(()=> 
                new ReviewTagger(buffer, sessionManager, builder)) as ITagger<T>;
        }
    }
}
