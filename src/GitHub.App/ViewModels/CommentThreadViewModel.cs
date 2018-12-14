using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base view model for a thread of comments.
    /// </summary>
    public abstract class CommentThreadViewModel : ReactiveObject, ICommentThreadViewModel
    {
        readonly ReactiveList<ICommentViewModel> comments = new ReactiveList<ICommentViewModel>();
        readonly Dictionary<ICommentViewModel, IObserver<ICommentViewModel>> draftThrottles =
            new Dictionary<ICommentViewModel, IObserver<ICommentViewModel>>();
        readonly IScheduler timerScheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="draftStore">The message draft store.</param>
        [ImportingConstructor]
        public CommentThreadViewModel(IMessageDraftStore draftStore)
            : this(draftStore, DefaultScheduler.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="draftStore">The message draft store.</param>
        /// <param name="timerScheduler">
        /// The scheduler to use to apply a throttle to message drafts.
        /// </param>
        [ImportingConstructor]
        public CommentThreadViewModel(
            IMessageDraftStore draftStore,
            IScheduler timerScheduler)
        {
            Guard.ArgumentNotNull(draftStore, nameof(draftStore));

            DraftStore = draftStore;
            this.timerScheduler = timerScheduler;
        }

        /// <inheritdoc/>
        public IReactiveList<ICommentViewModel> Comments => comments;

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; private set; }

        /// <inheritdoc/>
        IReadOnlyReactiveList<ICommentViewModel> ICommentThreadViewModel.Comments => comments;

        protected IMessageDraftStore DraftStore { get; }

        /// <inheritdoc/>
        public abstract Task PostComment(ICommentViewModel comment);

        /// <inheritdoc/>
        public abstract Task EditComment(ICommentViewModel comment);

        /// <inheritdoc/>
        public abstract Task DeleteComment(ICommentViewModel comment);

        /// <summary>
        /// Adds a placeholder comment that will allow the user to enter a reply, and wires up
        /// event listeners for saving drafts.
        /// </summary>
        /// <param name="placeholder">The placeholder comment view model.</param>
        /// <returns>An object which when disposed will remove the event listeners.</returns>
        protected IDisposable AddPlaceholder(ICommentViewModel placeholder)
        {
            Comments.Add(placeholder);

            return placeholder.WhenAnyValue(
                x => x.EditState,
                x => x.Body,
                (state, body) => (state, body))
                .Subscribe(x => PlaceholderChanged(placeholder, x.state));
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        protected Task InitializeAsync(ActorModel currentUser)
        {
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));
            CurrentUser = new ActorViewModel(currentUser);
            return Task.CompletedTask;
        }

        protected virtual CommentDraft BuildDraft(ICommentViewModel comment)
        {
            return !string.IsNullOrEmpty(comment.Body) ?
                new CommentDraft { Body = comment.Body } :
                null;
        }

        protected async Task DeleteDraft(ICommentViewModel comment)
        {
            if (draftThrottles.TryGetValue(comment, out var throttle))
            {
                throttle.OnCompleted();
                draftThrottles.Remove(comment);
            }

            var (key, secondaryKey) = GetDraftKeys(comment);
            await DraftStore.DeleteDraft(key, secondaryKey).ConfigureAwait(false);
        }

        protected abstract (string key, string secondaryKey) GetDraftKeys(ICommentViewModel comment);

        void PlaceholderChanged(ICommentViewModel placeholder, CommentEditState state)
        {
            if (state == CommentEditState.Editing)
            {
                if (!draftThrottles.TryGetValue(placeholder, out var throttle))
                {
                    var subject = new Subject<ICommentViewModel>();
                    subject.Throttle(TimeSpan.FromSeconds(1), timerScheduler).Subscribe(UpdateDraft);
                    draftThrottles.Add(placeholder, subject);
                    throttle = subject;
                }

                throttle.OnNext(placeholder);
            }
            else if (state != CommentEditState.Editing)
            {
                DeleteDraft(placeholder).Forget();
            }
        }

        void UpdateDraft(ICommentViewModel comment)
        {
            if (comment.EditState == CommentEditState.Editing)
            {
                var draft = BuildDraft(comment);
                var (key, secondaryKey) = GetDraftKeys(comment);

                if (draft != null)
                {
                    DraftStore.UpdateDraft(key, secondaryKey, draft).Forget();
                }
                else
                {
                    DraftStore.DeleteDraft(key, secondaryKey).Forget();
                }
            }
        }
    }
}
