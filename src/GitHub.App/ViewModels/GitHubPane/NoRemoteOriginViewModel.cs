using System;
using System.Reactive;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The view model for the "No Origin Remote" view in the GitHub pane.
    /// </summary>
    [Export(typeof(INoRemoteOriginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NoRemoteOriginViewModel : PanePageViewModelBase, INoRemoteOriginViewModel
    {
        ITeamExplorerServices teamExplorerServices;

        [ImportingConstructor]
        public NoRemoteOriginViewModel(ITeamExplorerServices teamExplorerServices)
        {
            this.teamExplorerServices = teamExplorerServices;
            EditRemotes = ReactiveCommand.CreateFromTask(teamExplorerServices.ShowRepositorySettingsRemotesAsync);
        }

        public ReactiveCommand<Unit, Unit> EditRemotes { get; }
    }
}
