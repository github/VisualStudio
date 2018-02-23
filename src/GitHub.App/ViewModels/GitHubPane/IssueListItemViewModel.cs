using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IIssueListItemViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueListItemViewModel : ViewModelBase, IIssueListItemViewModel
    {
        int number;
        string title;
        IActorViewModel author;
        DateTimeOffset updatedAt;

        public IssueListItemViewModel(IssueListModel model)
        {
            NodeId = model.NodeId;
            Number = model.Number;
            Title = model.Title;

            if (model.Author != null)
            {
                Author = new ActorViewModel(model.Author);
            }
        }

        public string NodeId { get; private set; }

        public int Number
        {
            get { return this.number; }
            private set { this.RaiseAndSetIfChanged(ref number, value); }
        }

        public string Title
        {
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public IActorViewModel Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
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
