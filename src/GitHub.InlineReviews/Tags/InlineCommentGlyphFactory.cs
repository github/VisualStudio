using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using GitHub.Factories;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.ViewModels;
using GitHub.Primitives;
using GitHub.InlineReviews.Glyph;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphFactory : IGlyphFactory<InlineCommentTag>
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IPeekBroker peekBroker;
        readonly ITextView textView;
        readonly ITagAggregator<InlineCommentTag> tagAggregator;

        public InlineCommentGlyphFactory(
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker,
            ITextView textView,
            ITagAggregator<InlineCommentTag> tagAggregator)
        {
            this.apiClientFactory = apiClientFactory;
            this.peekBroker = peekBroker;
            this.textView = textView;
            this.tagAggregator = tagAggregator;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;

            if (addTag != null)
            {
                return new AddInlineCommentGlyph();
            }
            else if (showTag != null)
            {
                return new ShowInlineCommentGlyph()
                {
                    Opacity = showTag.Thread.IsStale ? 0.5 : 1,
                };
            }

            return null;
        }

        public IEnumerable<Type> GetTagTypes()
        {
            return new[] { typeof(ShowInlineCommentTag), typeof(AddInlineCommentTag) };
        }

        public void PreprocessMouseLeftButtonUp(object source, MouseButtonEventArgs e)
        {
            var visualElement = (FrameworkElement)source;

            var y = e.GetPosition(visualElement).Y + textView.ViewportTop;
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
                thread = new InlineCommentThreadViewModel(
                    apiClient,
                    tag.Session,
                    showTag.Thread.OriginalCommitSha,
                    showTag.Thread.RelativePath,
                    showTag.Thread.OriginalPosition);

                foreach (var comment in showTag.Thread.Comments)
                {
                    thread.Comments.Add(new InlineCommentViewModel(thread, tag.Session.User, comment));
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
