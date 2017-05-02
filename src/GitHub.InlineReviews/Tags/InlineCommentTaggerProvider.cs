using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Tags
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(InlineCommentTag))]
    class InlineCommentTaggerProvider : IViewTaggerProvider
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IPullRequestReviewSessionManager sessionManager;

        [ImportingConstructor]
        public InlineCommentTaggerProvider(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IPullRequestReviewSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.sessionManager = sessionManager;
        }

        public ITagger<T> CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(()=> 
                new InlineCommentTagger(
                    gitService,
                    gitClient,
                    diffService,
                    view,
                    buffer,
                    sessionManager)) as ITagger<T>;
        }
    }
}
