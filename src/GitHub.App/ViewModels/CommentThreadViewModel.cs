using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.App.Models.Drafts;
using GitHub.Extensions;
using GitHub.Models;
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

            this.DraftStore = draftStore;
            this.timerScheduler = timerScheduler;

            comments.ChangeTrackingEnabled = true;
            comments.ItemChanged.Subscribe(ItemChanged);
        }

        /// <inheritdoc/>
        public IReactiveList<ICommentViewModel> Comments => comments;

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; private set; }

        /// <inheritdoc/>
        IReadOnlyReactiveList<ICommentViewModel> ICommentThreadViewModel.Comments => comments;

        protected IMessageDraftStore DraftStore { get; }

        /// <inheritdoc/>
        public abstract Task PostComment(string body);

        /// <inheritdoc/>
        public abstract Task EditComment(string id, string body);

        /// <inheritdoc/>
        public abstract Task DeleteComment(int pullRequestId, int commentId);

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

        protected abstract (string key, string secondaryKey) GetDraftKeys(ICommentViewModel comment);

        void ItemChanged(IReactivePropertyChangedEventArgs<ICommentViewModel> e)
        {
            if (e.PropertyName == nameof(ICommentViewModel.Body) &&
                e.Sender.EditState == CommentEditState.Editing)
            {
                if (!draftThrottles.TryGetValue(e.Sender, out var throttle))
                {
                    var subject = new Subject<ICommentViewModel>();
                    subject.Throttle(TimeSpan.FromSeconds(1), timerScheduler).Subscribe(UpdateDraft);
                    draftThrottles.Add(e.Sender, subject);
                    throttle = subject;
                }

                throttle.OnNext(e.Sender);
            }
            else if (e.PropertyName == nameof(ICommentViewModel.EditState) &&
                e.Sender.EditState != CommentEditState.Editing)
            {
                if (draftThrottles.TryGetValue(e.Sender, out var throttle))
                {
                    throttle.OnCompleted();
                    draftThrottles.Remove(e.Sender);
                }
            }
        }

        void UpdateDraft(ICommentViewModel comment)
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
