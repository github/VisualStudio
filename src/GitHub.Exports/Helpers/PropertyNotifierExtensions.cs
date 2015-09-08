using System.Runtime.CompilerServices;

namespace GitHub.VisualStudio.Helpers
{
    public static class PropertyNotifierExtensions
    {
        public static void RaisePropertyChange<TSender>(this TSender This, [CallerMemberName] string propertyName = null)
           where TSender : INotifyPropertySource
        {
            This.RaisePropertyChanged(propertyName);
        }
    }
}
