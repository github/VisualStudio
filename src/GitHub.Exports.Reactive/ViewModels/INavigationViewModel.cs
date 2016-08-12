using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface INavigationViewModel<TContent>
    {
        TContent Content { get; }
        ReactiveCommand<object> NavigateBack { get; }
        ReactiveCommand<object> NavigateForward { get; }

        bool Back();
        void Clear();
        bool Forward();
        void NavigateTo(TContent page);
    }
}