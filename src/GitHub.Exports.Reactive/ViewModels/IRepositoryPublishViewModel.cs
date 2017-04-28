using System.Reactive;
using GitHub.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace GitHub.ViewModels
{
    public interface IRepositoryPublishViewModel : IRepositoryForm
    {
        ObservableCollection<IConnection> Connections { get; }

        /// <summary>
        /// Command that creates the repository.
        /// </summary>
        IReactiveCommand<ProgressState> PublishRepository { get; }

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
