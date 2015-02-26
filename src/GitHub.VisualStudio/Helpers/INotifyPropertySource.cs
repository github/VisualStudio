namespace GitHub.VisualStudio.Helpers
{
    public interface INotifyPropertySource
    {
        void RaisePropertyChanged(string propertyName);
    }
}
