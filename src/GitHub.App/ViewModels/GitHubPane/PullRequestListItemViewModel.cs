using System;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model which displays an item in a <see cref="PullRequestListViewModel"/>.
    /// </summary>
    public class PullRequestListItemViewModel : ViewModelBase, IPullRequestListItemViewModel
    {
        bool isCurrent;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestListItemViewModel"/> class.
        /// </summary>
        /// <param name="model">The underlying pull request item model.</param>
        public PullRequestListItemViewModel(PullRequestListItemModel model)
        {
            Id = model.Id;
            Author = new ActorViewModel(model.Author);
            Checks = model.Checks;
            CommentCount = model.CommentCount;
            Number = model.Number;
            Title = model.Title;
            UpdatedAt = model.UpdatedAt;
        }

        /// <inheritdoc/>
        public string Id { get; protected set; }

        /// <inheritdoc/>
        public IActorViewModel Author { get; protected set; }

        /// <inheritdoc/>
        public PullRequestChecksState Checks { get; protected set; }

        /// <inheritdoc/>
        public int CommentCount { get; protected set; }

        /// <inheritdoc/>
        public bool IsCurrent
        {
            get { return isCurrent; }
            internal set { this.RaiseAndSetIfChanged(ref isCurrent, value); }
        }

        /// <inheritdoc/>
        public int Number { get; protected set; }

        /// <inheritdoc/>
        public string Title { get; protected set; }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt { get; protected set; }
    }
}
