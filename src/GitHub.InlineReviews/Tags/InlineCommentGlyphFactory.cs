using System;
using System.Windows;
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
            var glyph = CreateGlyph(tag);
            glyph.MouseLeftButtonUp += (s, e) => OpenThreadView(line, tag);

            return glyph;
        }

        public IEnumerable<Type> GetTagTypes()
        {
            return new[]
            {
                typeof(AddInlineCommentTag),
                typeof(ShowInlineCommentTag)
            };
        }

        static UIElement CreateGlyph(InlineCommentTag tag)
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

            throw new ArgumentException($"Unknown 'InlineCommentTag' type '{tag}'");
        }

        void OpenThreadView(ITextViewLine line, InlineCommentTag tag)
        {
            var trackingPoint = textView.TextSnapshot.CreateTrackingPoint(line.Start.Position, PointTrackingMode.Positive);
            var viewModel = CreateViewModel(tag);
            var options = new InlineCommentPeekSessionCreationOptions(textView, trackingPoint, viewModel);
            peekBroker.TriggerPeekSession(options);
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
