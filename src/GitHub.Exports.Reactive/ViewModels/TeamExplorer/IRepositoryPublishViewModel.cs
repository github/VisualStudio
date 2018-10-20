using System.Reactive;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.TeamExplorer
{
    public interface IRepositoryPublishViewModel : IViewModel, IRepositoryForm
    {
        IReadOnlyObservableCollection<IConnection> Connections { get; }

        /// <summary>
        /// Gets the busy state of the publish.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Command that creates the repository.
        /// </summary>
        ReactiveCommand<Unit, ProgressState> PublishRepository { get; }

        /// <summary>
        /// Determines whether the host combo box is visible. Only true if the user is logged into more than one host.
        /// </summary>
        bool IsHostComboBoxVisible { get; }

        /// <summary>
        /// The selected host to publish to.
        /// </summary>
        IConnection SelectedConnection { get; set; }
    }

    public enum ProgressState
    {
        Idle,
        Running,
        Success,
        Fail
    }
}
