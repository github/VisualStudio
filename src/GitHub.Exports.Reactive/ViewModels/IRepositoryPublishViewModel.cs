using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IRepositoryPublishViewModel : IRepositoryForm
    {
        ReactiveList<IConnection> Connections { get; }

        /// <summary>
        /// Command that creates the repository.
        /// </summary>
        IReactiveCommand<ProgressState> PublishRepository { get; }

        /// <summary>
        /// True when publishing is in progress.
        /// </summary>
        bool IsPublishing { get; }

        /// <summary>
        /// Determines whether the host combo box is visible. Only true if the user is logged into more than one host.
        /// </summary>
        bool IsHostComboBoxVisible { get; }

        /// <summary>
        /// The selected host to publish to.
        /// </summary>
        IConnection SelectedConnection { get; set; }

        /// <summary>
        /// Sets the default repository name when prepopulating the form.
        /// </summary>
        string DefaultRepositoryName { get; }
    }

    public enum ProgressState
    {
        Idle,
        Running,
        Success,
        Fail
    }

}
