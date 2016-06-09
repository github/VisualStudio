using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IReactiveViewModel : IViewModel
    {
        ReactiveCommand<object> CancelCommand { get; }
    }
}