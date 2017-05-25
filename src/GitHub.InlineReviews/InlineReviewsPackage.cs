using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Views;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(UIContextGuids80.CodeWindow)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(PullRequestCommentsPane), DocumentLikeTool=true)]
    public class InlineReviewsPackage : Package
    {
        public async void ShowPullRequestComments(IPullRequestModel pullRequest)
        {
            var window = (PullRequestCommentsPane)FindToolWindow(
                typeof(PullRequestCommentsPane), pullRequest.Number, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

            var serviceProvider = (IGitHubServiceProvider)GetGlobalService(typeof(IGitHubServiceProvider));
            var manager = serviceProvider.GetService<IPullRequestSessionManager>();
            var session = await manager.GetSession(pullRequest);
            var address = HostAddress.Create(session.Repository.CloneUrl);
            var apiClientFactory = serviceProvider.GetService<IApiClientFactory>();
            var apiClient = apiClientFactory.Create(address);
            await window.Initialize(session, apiClient);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public void NextInlineComment()
        {
            var peekService = GetPeekService();
            if (peekService == null) return;

            var textView = GetCurrentTextView();
            if (textView == null) return;

            var tagAggregator = GetTagAggregator<InlineCommentTag>(textView);
            if (tagAggregator == null) return;

            var cursorPoint = textView.Caret.Position.BufferPosition.Position;
            var span = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);
            var mappingSpan = textView.BufferGraph.CreateMappingSpan(span, SpanTrackingMode.EdgeExclusive);

            var tags = tagAggregator.GetTags(mappingSpan)
                .Select(x => new
                {
                    Point = Map(x.Span.Start, textView.TextSnapshot),
                    Tag = x.Tag as ShowInlineCommentTag,
                })
                .Where(x => x.Tag != null && x.Point.HasValue)
                .OrderBy(x => x.Point.Value)
                .ToList();
            if (tags.Count == 0) return;

            var next = tags.FirstOrDefault(x => x.Point > cursorPoint) ?? tags.First();
            peekService.Show(next.Tag, true);
        }

        public void PreviousInlineComment()
        {
            var peekService = GetPeekService();
            if (peekService == null) return;

            var textView = GetCurrentTextView();
            if (textView == null) return;

            var tagAggregator = GetTagAggregator<InlineCommentTag>(textView);
            if (tagAggregator == null) return;

            var cursorPoint = textView.Caret.Position.BufferPosition.Position;
            var span = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);
            var mappingSpan = textView.BufferGraph.CreateMappingSpan(span, SpanTrackingMode.EdgeExclusive);

            var tags = tagAggregator.GetTags(mappingSpan)
                .Select(x => new
                {
                    Point = Map(x.Span.Start, textView.TextSnapshot),
                    Tag = x.Tag as ShowInlineCommentTag,
                })
                .Where(x => x.Tag != null && x.Point.HasValue)
                .OrderBy(x => x.Point.Value)
                .ToList();
            if (tags.Count == 0) return;

            var next = tags.LastOrDefault(x => x.Point < cursorPoint) ?? tags.Last();
            peekService.Show(next.Tag, true);
        }

        private int? Map(IMappingPoint p, ITextSnapshot textSnapshot)
        {
            return p.GetPoint(textSnapshot.TextBuffer, PositionAffinity.Predecessor);
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.AddCommandHandler<IPullRequestModel>(
                GlobalCommands.CommandSetGuid,
                GlobalCommands.ShowPullRequestCommentsId,
                () => true,
                ShowPullRequestComments);
            this.AddCommandHandler(
                GlobalCommands.CommandSetGuid,
                GlobalCommands.NextInlineCommentId,
                (s, e) => NextInlineComment());
            this.AddCommandHandler(
                GlobalCommands.CommandSetGuid,
                GlobalCommands.PreviousInlineCommentId,
                (s, e) => PreviousInlineComment());
        }

        IWpfTextView GetCurrentTextView()
        {
            var textManager = (IVsTextManager)GetGlobalService(typeof(VsTextManagerClass));
            IVsTextView textView = null;
            textManager.GetActiveView(0, null, out textView);
            var userData = textView as IVsUserData;
            if (userData == null) return null;
            IWpfTextViewHost viewHost;
            object holder;
            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out holder);
            viewHost = (IWpfTextViewHost)holder;
            return viewHost?.TextView;
        }

        IInlineCommentPeekService GetPeekService()
        {
            var serviceProvider = (IGitHubServiceProvider)GetGlobalService(typeof(IGitHubServiceProvider));
            return serviceProvider.GetService<IInlineCommentPeekService>();
        }

        ITagAggregator<T> GetTagAggregator<T>(ITextView textView) where T : ITag
        {
            var serviceProvider = (IGitHubServiceProvider)GetGlobalService(typeof(IGitHubServiceProvider));
            var factory = serviceProvider.GetService<IViewTagAggregatorFactoryService>();
            return factory.CreateTagAggregator<T>(textView);
        }
    }
}
