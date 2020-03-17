using System;
using System.Windows.Input;
using System.ComponentModel;

namespace GitHub.InlineReviews.ViewModels
{
    public class PullRequestStatusViewModel : INotifyPropertyChanged
    {
        int? number;
        string title;
        string repositoryName;
        string repositoryOwner;

        public PullRequestStatusViewModel(ICommand openPullRequestsCommand, ICommand showCurrentPullRequestCommand)
        {
            OpenPullRequestsCommand = openPullRequestsCommand;
            ShowCurrentPullRequestCommand = showCurrentPullRequestCommand;
        }

        public int? Number
        {
            get { return number; }
            set
            {
                if (number != value)
                {
                    number = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number)));
                }
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public string RepositoryOwner
        {
            get { return repositoryOwner; }
            set
            {
                if (repositoryOwner != value)
                {
                    repositoryOwner = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryOwner)));
                }
            }
        }

        public string RepositoryName
        {
            get { return repositoryName; }
            set
            {
                if (repositoryName != value)
                {
                    repositoryName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepositoryName)));
                }
            }
        }

        public ICommand OpenPullRequestsCommand { get; }
        public ICommand ShowCurrentPullRequestCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
