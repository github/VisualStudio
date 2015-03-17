using ReactiveUI;

namespace GitHub.Exports
{
    public interface ISelectable : IReactiveNotifyPropertyChanged<IReactiveObject>
    {
        bool IsSelected { get; set; }
    }
}
