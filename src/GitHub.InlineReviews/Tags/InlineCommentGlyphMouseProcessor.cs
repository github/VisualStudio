using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GitHub.Factories;
using GitHub.InlineReviews.Models;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.ViewModels;
using GitHub.Primitives;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphMouseProcessor : MouseProcessorBase
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekBroker peekBroker;
        readonly ITextView textView;
        readonly IWpfTextViewMargin margin;
        readonly ITagAggregator<InlineCommentTag> tagAggregator;

        public InlineCommentGlyphMouseProcessor(
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker,
            ITextView textView,
            IWpfTextViewMargin margin,
            ITagAggregator<InlineCommentTag> aggregator)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
            this.textView = textView;
            this.margin = margin;
            this.tagAggregator = aggregator;
        }

        public override void PreprocessMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var y = e.GetPosition(margin.VisualElement).Y + textView.ViewportTop;
            var line = textView.TextViewLines.GetTextViewLineContainingYCoordinate(y);

            if (line != null)
            {
                var tag = tagAggregator.GetTags(line.ExtentAsMappingSpan).FirstOrDefault();

                if (tag != null)
                {
                    var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
                    var viewModel = CreateViewModel(tag.Tag);
                    var options = new InlineCommentPeekSessionCreationOptions(textView, trackingPoint, viewModel);
                    peekBroker.TriggerPeekSession(options);
                    e.Handled = true;
                }
            }
        }

        InlineCommentThreadViewModel CreateViewModel(InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;
            var repository = tag.Session.Repository;
            var apiClient = apiClientFactory.Create(HostAddress.Create(repository.CloneUrl.Host));
            InlineCommentThreadViewModel thread;

            if (addTag != null)
            {
                thread = new InlineCommentThreadViewModel(
                    apiClient,
                    tag.Session,
                    addTag.CommitSha,
                    addTag.FilePath,
                    addTag.DiffLine);
                var placeholder = thread.AddReplyPlaceholder();
                placeholder.BeginEdit.Execute(null);
            }
            else if (showTag != null)
            {
                var firstComment = showTag.Comments.First();

                thread = new InlineCommentThreadViewModel(
                    apiClient,
                    tag.Session,
                    firstComment.Original.OriginalCommitId,
                    firstComment.Original.Path,
                    firstComment.Original.OriginalPosition.Value);

                foreach (var comment in showTag.Comments)
                {
                    thread.Comments.Add(new InlineCommentViewModel(thread, tag.Session.User, comment.Original));
                }

                thread.AddReplyPlaceholder();
            }
            else
            {
                throw new NotSupportedException("Unrecognised inline comment tag.");
            }

            return thread;
        }
    }
}
