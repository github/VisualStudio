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
    [TagType(typeof(ShowInlineCommentTag))]
    class InlineCommentTaggerProvider : IViewTaggerProvider
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IInlineCommentPeekService peekService;

        [ImportingConstructor]
        public InlineCommentTaggerProvider(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IPullRequestSessionManager sessionManager,
            IInlineCommentPeekService peekService)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(peekService, nameof(peekService));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.sessionManager = sessionManager;
            this.peekService = peekService;
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
                    sessionManager,
                    peekService)) as ITagger<T>;
        }
    }
}
