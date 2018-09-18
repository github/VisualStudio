using System;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model which displays an item in a <see cref="IssueListViewModel"/>.
    /// </summary>
    public class IssueListItemViewModel : ViewModelBase, IIssueListItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssueListItemViewModel"/> class.
        /// </summary>
        /// <param name="model">The underlying issue item model.</param>
        public IssueListItemViewModel(IssueListItemModel model)
        {
            Id = model.Id;
            Author = new ActorViewModel(model.Author);
            CommentCount = model.CommentCount;
            Number = model.Number;
            Title = model.Title;
            UpdatedAt = model.UpdatedAt;
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public IActorViewModel Author { get; }

        /// <inheritdoc/>
        public int CommentCount { get; }

        /// <inheritdoc/>
        public int Number { get; }

        /// <inheritdoc/>
        public string Title { get; }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt { get; }
    }
}
