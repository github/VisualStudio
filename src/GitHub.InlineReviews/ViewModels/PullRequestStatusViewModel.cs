using System;
using System.Windows.Input;
using System.ComponentModel;

namespace GitHub.InlineReviews.ViewModels
{
    public class PullRequestStatusViewModel : INotifyPropertyChanged
    {
        int? number;
        string title;

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

        public ICommand OpenPullRequestsCommand { get; }
        public ICommand ShowCurrentPullRequestCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
