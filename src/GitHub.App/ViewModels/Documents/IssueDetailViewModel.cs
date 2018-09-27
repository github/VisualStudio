using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model that displays issue details in a document pane.
    /// </summary>
    [Export(typeof(IIssueDetailViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueDetailViewModel : ViewModelBase, IIssueDetailViewModel, ICommentThreadViewModel
    {
        readonly IIssueService service;
        readonly ICommentService commentService;
        readonly IPullRequestSessionManager sessionManager;
        int number;
        string title;
        ICommentViewModel body;
        ObservableCollection<ICommentViewModel> comments;

        [ImportingConstructor]
        public IssueDetailViewModel(
            IIssueService service,
            ICommentService commentService,
            IPullRequestSessionManager sessionManager)
        {
            this.service = service;
            this.commentService = commentService;
            this.sessionManager = sessionManager;
        }

        public int Number
        {
            get => number;
            private set => this.RaiseAndSetIfChanged(ref number, value);
        }

        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public ICommentViewModel Body
        {
            get => body;
            private set => this.RaiseAndSetIfChanged(ref body, value);
        }

        public ObservableCollection<ICommentViewModel> Comments
        {
            get => comments;
            private set => this.RaiseAndSetIfChanged(ref comments, value);
        }

        public IActorViewModel CurrentUser => throw new NotImplementedException();

        public ReactiveCommand<Unit> PostComment => throw new NotImplementedException();

        public ReactiveCommand<Unit> EditComment => throw new NotImplementedException();

        public ReactiveCommand<Unit> DeleteComment => throw new NotImplementedException();

        public Task InitializeAsync(IServiceProvider paneServiceProvider) => Task.CompletedTask;

        public async Task InitializeAsync(
            IConnection connection,
            string owner,
            string repo,
            int number)
        {
            ////IsLoading = true;
            Number = number;

            try
            {
                var model = await service.ReadIssue(
                    connection.HostAddress,
                    owner,
                    repo,
                    number).ConfigureAwait(true);
                await Load(model).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                ////Error = ex;
            }
            finally
            {
                ////IsLoading = false;
            }
        }

        Task Load(IssueDetailModel model)
        {
            Title = model.Title;
            Body = new CommentViewModel(
                commentService,
                this,
                new ActorViewModel(),
                0,
                model.Id,
                0,
                model.Body,
                CommentEditState.None,
                new ActorViewModel(model.Author),
                model.UpdatedAt,
                new Uri("https://github.com"));
            return Task.CompletedTask;
        }
    }
}
