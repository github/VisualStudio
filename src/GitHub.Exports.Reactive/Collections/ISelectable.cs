using ReactiveUI;

namespace GitHub.Collections
{
    public interface ISelectable : IReactiveNotifyPropertyChanged<IReactiveObject>
    {
        bool IsSelected { get; set; }
    }
}
