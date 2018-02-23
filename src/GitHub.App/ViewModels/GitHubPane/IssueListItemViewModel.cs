using System;
using System.Collections.ObjectModel;
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
        int number;
        IssueState state;
        string title;
        DateTimeOffset updatedAt;

        public IssueListItemViewModel(IssueListModel model)
        {
            NodeId = model.NodeId;
            Number = model.Number;
            State = model.State;
            Title = model.Title;

            if (model.Author != null)
            {
                Author = new ActorViewModel(model.Author);
            }

            Assignees = new ObservableCollection<IActorViewModel>(
                model.Assignees.Select(x => new ActorViewModel(x)));
        }

        public ObservableCollection<IActorViewModel> Assignees { get; }

        public IActorViewModel Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

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
