using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IReactiveViewModel : IViewModel
    {
        IReactiveCommand<object> CancelCommand { get; }
    }
}