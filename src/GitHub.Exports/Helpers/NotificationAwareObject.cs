using System.ComponentModel;
using GitHub.VisualStudio.Helpers;

namespace GitHub.Primitives
{
    public abstract class NotificationAwareObject : INotifyPropertyChanged, INotifyPropertySource
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}