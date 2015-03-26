using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IRepositoryPublishViewModel : IRepositoryForm
    {
        ReactiveList<IRepositoryHost> RepositoryHosts { get; }

        /// <summary>
        /// Command that creates the repository.
        /// </summary>
        IReactiveCommand<Unit> PublishRepository { get; }

        /// <summary>
        /// True when publishing is in progress.
        /// </summary>
        bool IsPublishing { get; }

        /// <summary>
        /// The selected host to publish to.
        /// </summary>
        IRepositoryHost SelectedHost { get; set; }
    }
}
