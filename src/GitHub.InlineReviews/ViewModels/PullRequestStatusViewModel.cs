using System;
using System.ComponentModel;

namespace GitHub.InlineReviews.ViewModels
{
    public class PullRequestStatusViewModel : INotifyPropertyChanged
    {
        int? number;
        string title;

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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
