using System.ComponentModel;
using System.Runtime.CompilerServices;
using GitHub.VisualStudio.Helpers;

namespace GitHub.Primitives
{
    public abstract class NotificationAwareObject : INotifyPropertyChanged, INotifyPropertySource
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}