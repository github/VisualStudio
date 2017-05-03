using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GitHub.Factories;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.ViewModels;
using GitHub.Primitives;
using GitHub.Services;
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

        private CommentThreadViewModel CreateViewModel(InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;
            var repository = tag.Session.Repository;
            var apiClient = apiClientFactory.Create(HostAddress.Create(repository.CloneUrl.Host));

            if (addTag != null)
            {
                return new CommentThreadViewModel(
                    apiClient,
                    tag.Session,
                    addTag.CommitSha,
                    addTag.FilePath,
                    addTag.DiffLine);
            }
            else if (showTag != null)
            {
                return new CommentThreadViewModel(apiClient, tag.Session, showTag.Comments);
            }

            throw new NotSupportedException("Unsupported tag type.");
        }
    }
}
