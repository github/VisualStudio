using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    [Export(typeof(IInlineCommentPeekService))]
    class InlineCommentPeekService : IInlineCommentPeekService
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IOutliningManagerService outliningService;
        readonly IPeekBroker peekBroker;

        [ImportingConstructor]
        public InlineCommentPeekService(
            IApiClientFactory apiClientFactory,
            IOutliningManagerService outliningManager,
            IPeekBroker peekBroker)
        {
            this.apiClientFactory = apiClientFactory;
            this.outliningService = outliningManager;
            this.peekBroker = peekBroker;
        }

        /// <inheritdoc/>
        public void Show(AddInlineCommentTag tag, bool moveCaret = false)
        {
            Guard.ArgumentNotNull(tag, nameof(tag));

            var textView = tag.TextView;
            var line = textView.TextSnapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            var viewModel = new InlineCommentThreadViewModel(
                CreateApiClient(tag.Session.Repository),
                tag.Session,
                tag.CommitSha,
                tag.FilePath,
                tag.DiffLine);
            var placeholder = viewModel.AddReplyPlaceholder();
            placeholder.BeginEdit.Execute(null);

            ExpandCollapsedRegions(textView, line.Extent);
            if (moveCaret) textView.Caret.MoveTo(line.Start);

            var options = new InlineCommentPeekSessionCreationOptions(textView, trackingPoint, viewModel);
            peekBroker.TriggerPeekSession(options);
        }

        /// <inheritdoc/>
        public void Show(ShowInlineCommentTag tag, bool moveCaret = false)
        {
            Guard.ArgumentNotNull(tag, nameof(tag));

            var textView = tag.TextView;
            var line = textView.TextSnapshot.GetLineFromLineNumber(tag.LineNumber);
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            var viewModel = new InlineCommentThreadViewModel(
                CreateApiClient(tag.Session.Repository),
                tag.Session,
                tag.Thread.OriginalCommitSha,
                tag.Thread.RelativePath,
                tag.Thread.OriginalPosition);

            foreach (var comment in tag.Thread.Comments)
            {
                viewModel.Comments.Add(new InlineCommentViewModel(viewModel, tag.Session.User, comment));
            }

            viewModel.AddReplyPlaceholder();

            ExpandCollapsedRegions(textView, line.Extent);
            if (moveCaret) textView.Caret.MoveTo(line.Start);

            var options = new InlineCommentPeekSessionCreationOptions(textView, trackingPoint, viewModel);
            peekBroker.TriggerPeekSession(options);
        }

        IApiClient CreateApiClient(ILocalRepositoryModel repository)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl.Host);
            return apiClientFactory.Create(hostAddress);
        }

        void ExpandCollapsedRegions(ITextView textView, SnapshotSpan span)
        {
            var outlining = outliningService.GetOutliningManager(textView);

            if (outlining != null)
            {
                foreach (var collapsed in outlining.GetCollapsedRegions(span))
                {
                    outlining.Expand(collapsed);
                }
            }
        }
    }
}
