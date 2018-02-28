using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IIssueListItemViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueListItemViewModel : ViewModelBase, IIssueListItemViewModel
    {
        IActorViewModel author;
        int commentCount;
        int number;
        IssueState state;
        string title;
        DateTimeOffset updatedAt;

        public IssueListItemViewModel(IssueListModel model)
        {
            NodeId = model.NodeId;
            number = model.Number;
            state = model.State;
            title = model.Title;
            commentCount = model.CommentCount;
            updatedAt = model.UpdatedAt;

            if (model.Author != null)
            {
                author = new ActorViewModel(model.Author);
            }

            Labels = model.Labels.Select(x => new LabelViewModel(x)).ToList();
            Assignees = model.Assignees.Select(x => new ActorViewModel(x)).ToList();
        }

        public IReadOnlyList<IActorViewModel> Assignees { get; }

        public IActorViewModel Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

        public int CommentCount
        {
            get { return commentCount; }
            private set { this.RaiseAndSetIfChanged(ref commentCount, value); }
        }

        public IReadOnlyList<ILabelViewModel> Labels { get; }

        public string NodeId { get; private set; }

        public int Number
        {
            get { return this.number; }
            private set { this.RaiseAndSetIfChanged(ref number, value); }
        }

        public IssueState State
        {
            get { return this.state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        public string Title
        {
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public DateTimeOffset UpdatedAt
        {
            get { return updatedAt; }
            private set { this.RaiseAndSetIfChanged(ref updatedAt, value); }
        }

        public void CopyFrom(IIssueListItemViewModel other)
        {
            NodeId = other.NodeId;
            Number = other.Number;
            Title = other.Title;
            Author = other.Author;
            UpdatedAt = other.UpdatedAt;
        }
    }
}
