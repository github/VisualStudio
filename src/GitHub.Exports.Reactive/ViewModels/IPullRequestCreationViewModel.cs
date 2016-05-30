using ReactiveUI;

namespace GitHub.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IPullRequestCreationViewModel : IViewModel
    {
        ReactiveCommand<object> CancelCommand { get; }
    }
}
