using System;
using System.Collections.Generic;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public class PullRequestListItemViewModel : ViewModelBase, IPullRequestListItemViewModel
    {
        bool isCurrent;

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

        public bool IsCurrent
        {
            get { return isCurrent; }
            internal set { this.RaiseAndSetIfChanged(ref isCurrent, value); }
        }

        public IReadOnlyList<ILabelViewModel> Labels { get; protected set; }

        public int Number { get; protected set; }

        public string Title { get; protected set; }

        public DateTimeOffset UpdatedAt { get; protected set; }
    }
}
