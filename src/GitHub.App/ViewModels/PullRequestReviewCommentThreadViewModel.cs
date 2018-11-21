using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using static System.FormattableString;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A thread of pull request review comments.
    /// </summary>
    [Export(typeof(IPullRequestReviewCommentThreadViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewCommentThreadViewModel : CommentThreadViewModel, IPullRequestReviewCommentThreadViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly ObservableAsPropertyHelper<bool> needsPush;
        IPullRequestSessionFile file;
        bool isNewThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="draftStore">The message draft store.</param>
        /// <param name="factory">The view model factory.</param>
        [ImportingConstructor]
        public PullRequestReviewCommentThreadViewModel(
            IMessageDraftStore draftStore,
            IViewViewModelFactory factory)
            : base(draftStore)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));

            this.factory = factory;

            needsPush = this.WhenAnyValue(
                x => x.File.CommitSha,
                x => x.IsNewThread,
                (sha, isNew) => isNew && sha == null)
                .ToProperty(this, x => x.NeedsPush);
        }

        /// <inheritdoc/>
        public IPullRequestSession Session { get; private set; }

        /// <inheritdoc/>
        public IPullRequestSessionFile File
        {
            get => file;
            private set => this.RaiseAndSetIfChanged(ref file, value);
        }

        /// <inheritdoc/>
        public int LineNumber { get; private set; }

        /// <inheritdoc/>
        public DiffSide Side { get; private set; }

        public bool IsNewThread
        {
            get => isNewThread;
            private set => this.RaiseAndSetIfChanged(ref isNewThread, value);
        }

        /// <inheritdoc/>
        public bool NeedsPush => needsPush.Value;

        /// <inheritdoc/>
        public async Task InitializeAsync(IPullRequestSession session,
            IPullRequestSessionFile file,
            IInlineCommentThreadModel thread,
            bool addPlaceholder)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            await base.InitializeAsync(session.User).ConfigureAwait(true);

            Session = session;
            File = file;
            LineNumber = thread.LineNumber;
            Side = thread.DiffLineType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right;

            foreach (var comment in thread.Comments)
            {
                var vm = factory.CreateViewModel<IPullRequestReviewCommentViewModel>();
                await vm.InitializeAsync(
                    session,
                    this,
                    comment.Review,
                    comment.Comment,
                    CommentEditState.None).ConfigureAwait(false);
                Comments.Add(vm);
            }

            if (addPlaceholder)
            {
                var vm = factory.CreateViewModel<IPullRequestReviewCommentViewModel>();

                await vm.InitializeAsPlaceholderAsync(
                    session,
                    this,
                    session.HasPendingReview,
                    false).ConfigureAwait(true);

                var (key, secondaryKey) = GetDraftKeys(vm);
                var draft = await DraftStore.GetDraft<PullRequestReviewCommentDraft>(key, secondaryKey).ConfigureAwait(true);

                if (draft?.Side == Side)
                {
                    await vm.BeginEdit.Execute();
                    vm.Body = draft.Body;
                }

                AddPlaceholder(vm);
            }
        }

        /// <inheritdoc/>
        public async Task InitializeNewAsync(
            IPullRequestSession session,
            IPullRequestSessionFile file,
            int lineNumber,
            DiffSide side,
            bool isEditing)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            await base.InitializeAsync(session.User).ConfigureAwait(false);

            Session = session;
            File = file;
            LineNumber = lineNumber;
            Side = side;
            IsNewThread = true;

            var vm = factory.CreateViewModel<IPullRequestReviewCommentViewModel>();
            await vm.InitializeAsPlaceholderAsync(session, this, session.HasPendingReview, isEditing).ConfigureAwait(false);

            var (key, secondaryKey) = GetDraftKeys(vm);
            var draft = await DraftStore.GetDraft<PullRequestReviewCommentDraft>(key, secondaryKey).ConfigureAwait(true);

            if (draft?.Side == side)
            {
                vm.Body = draft.Body;
            }

            AddPlaceholder(vm);
        }

        public override async Task PostComment(ICommentViewModel comment)
        {
            Guard.ArgumentNotNull(comment, nameof(comment));

            await DeleteDraft(comment).ConfigureAwait(false);

            try
            {
                if (IsNewThread)
                {
                    var diffPosition = File.Diff
                        .SelectMany(x => x.Lines)
                        .FirstOrDefault(x =>
                        {
                            var line = Side == DiffSide.Left ? x.OldLineNumber : x.NewLineNumber;
                            return line == LineNumber + 1;
                        });

                    if (diffPosition == null)
                    {
                        throw new InvalidOperationException("Unable to locate line in diff.");
                    }

                    await Session.PostReviewComment(
                        comment.Body,
                        File.CommitSha,
                        File.RelativePath.Replace("\\", "/"),
                        File.Diff,
                        diffPosition.DiffLineNumber).ConfigureAwait(false);
                }
                else
                {
                    var replyId = Comments[0].Id;
                    await Session.PostReviewComment(comment.Body, replyId).ConfigureAwait(false);
                }
            }
            catch
            {
                UpdateDraft(comment).Forget();
                throw;
            }
        }

        public override async Task EditComment(ICommentViewModel comment)
        {
            Guard.ArgumentNotNull(comment, nameof(comment));

            await Session.EditComment(comment.Id, comment.Body).ConfigureAwait(false);
        }

        public override async Task DeleteComment(ICommentViewModel comment)
        {
            Guard.ArgumentNotNull(comment, nameof(comment));

            await Session.DeleteComment(comment.PullRequestId, comment.DatabaseId).ConfigureAwait(false);
        }

        public static (string key, string secondaryKey) GetDraftKeys(
            UriString cloneUri,
            int pullRequestNumber,
            string relativePath,
            int lineNumber)
        {
            relativePath = relativePath.Replace("\\", "/");
            var key = Invariant($"pr-review-comment|{cloneUri}|{pullRequestNumber}|{relativePath}");
            return (key, lineNumber.ToString(CultureInfo.InvariantCulture));
        }

        protected override CommentDraft BuildDraft(ICommentViewModel comment)
        {
            return !string.IsNullOrEmpty(comment.Body) ?
                new PullRequestReviewCommentDraft
                {
                    Body = comment.Body,
                    Side = Side,
                    UpdatedAt = DateTimeOffset.UtcNow,
                } : null;
        }

        protected override (string key, string secondaryKey) GetDraftKeys(ICommentViewModel comment)
        {
            return GetDraftKeys(
                Session.LocalRepository.CloneUrl.WithOwner(Session.RepositoryOwner),
                Session.PullRequest.Number,
                File.RelativePath,
                LineNumber);
        }
        
        async Task UpdateDraft(ICommentViewModel comment)
        {
            var draft = BuildDraft(comment);
            var (key, secondaryKey) = GetDraftKeys(comment);
            await DraftStore.UpdateDraft(key, secondaryKey, draft).ConfigureAwait(true);
        }
    }
}
