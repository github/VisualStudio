using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
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
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IPullRequestReviewSessionManager sessionManager;

        [ImportingConstructor]
        public ReviewTaggerProvider(
            IGitService gitService,
            IGitClient gitClient,
            IPullRequestReviewSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.sessionManager = sessionManager;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(()=> 
                new ReviewTagger(gitService, gitClient, buffer, sessionManager)) as ITagger<T>;
        }
    }
}
