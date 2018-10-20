using System;
using System.ComponentModel.Composition;
using System.Reactive;
using GitHub.Services;
using ReactiveUI;
using UserError = ReactiveUI.Legacy.UserError;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The view model for the "Login Failed" view in the GitHub pane.
    /// </summary>
    [Export(typeof(ILoginFailedViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoginFailedViewModel : PanePageViewModelBase, ILoginFailedViewModel
    {
        readonly ITeamExplorerServices teServices;
        UserError loginError;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginFailedViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public LoginFailedViewModel(ITeamExplorerServices teServices)
        {
            this.teServices = teServices;
            OpenTeamExplorer = ReactiveCommand.Create(DoOpenTeamExplorer);
        }

        /// <inheritdoc/>
        public UserError LoginError
        {
            get => loginError;
            private set => this.RaiseAndSetIfChanged(ref loginError, value);
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> OpenTeamExplorer { get; }

        public void Initialize(UserError error)
        {
            LoginError = error;
        }

        void DoOpenTeamExplorer() => teServices.ShowConnectPage();
    }
}