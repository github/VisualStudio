using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public class PullRequestListItemViewModel : ViewModelBase, IPullRequestListItemViewModel
    {
        public PullRequestListItemViewModel(PullRequestListItemModel model)
        {
            Id = model.Id;
            Author = new ActorViewModel(model.Author);
            CommentCount = model.CommentCount;
            Number = model.Number;
            Title = model.Title;
            UpdatedAt = model.UpdatedAt;
        }

        public string Id { get; protected set; }

        public IActorViewModel Author { get; protected set; }

        public int CommentCount { get; protected set; }

        public IReadOnlyList<ILabelViewModel> Labels { get; protected set; }

        public int Number { get; protected set; }

        public string Title { get; protected set; }

        public DateTimeOffset UpdatedAt { get; protected set; }
    }
}
