#pragma warning disable CA1030 // Use events where appropriate

namespace GitHub.VisualStudio.Helpers
{
    public interface INotifyPropertySource
    {
        void RaisePropertyChanged(string propertyName);
    }
}
