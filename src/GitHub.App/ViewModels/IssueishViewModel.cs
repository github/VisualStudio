using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for issue and pull request view models.
    /// </summary>
    public class IssueishViewModel : ViewModelBase, IIssueishViewModel
    {
        static readonly ILogger log = LogManager.ForContext<IssueishViewModel>();

        IActorViewModel author;
        string body;
        string title;
        Uri webUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueishViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public IssueishViewModel()
        {
        }

        /// <inheritdoc/>
        public IRemoteRepositoryModel Repository { get; private set; }

        /// <inheritdoc/>
        public int Number { get; private set; }

        /// <inheritdoc/>
        public IActorViewModel Author
        {
            get => author;
            private set => this.RaiseAndSetIfChanged(ref author, value);
        }

        /// <inheritdoc/>
        public string Body
        {
            get => body;
            protected set => this.RaiseAndSetIfChanged(ref body, value);
        }

        /// <inheritdoc/>
        public string Title
        {
            get => title;
            protected set => this.RaiseAndSetIfChanged(ref title, value);
        }

        /// <inheritdoc/>
        public Uri WebUrl
        {
            get { return webUrl; }
            private set { this.RaiseAndSetIfChanged(ref webUrl, value); }
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }

        protected Task InitializeAsync(
            IRemoteRepositoryModel repository,
            IssueishDetailModel model)
        {
            Repository = repository;
            Author = new ActorViewModel(model.Author);
            Body = model.Body;
            Number = model.Number;
            Title = model.Title;
            return Task.CompletedTask;
        }
    }
}
