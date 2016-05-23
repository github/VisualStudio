using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a view model for the "Log out Required" dialog..
    /// </summary>
    public interface ILogoutRequiredViewModel : IViewModel
    {
        IReactiveCommand<ProgressState> Logout { get; }

        ReactiveCommand<object> CancelCommand { get; }
    }
}