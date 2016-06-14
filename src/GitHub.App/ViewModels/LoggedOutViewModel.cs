using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using System.Reactive.Linq;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Subjects;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// The view model for the "Sign in to GitHub" view in the GitHub pane.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.LoggedOut)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoggedOutViewModel : BaseViewModel, ILoggedOutViewModel
    {
        IUIProvider uiProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggedOutViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public LoggedOutViewModel(IUIProvider uiProvider)
        {
            this.uiProvider = uiProvider;
            SignIn = ReactiveCommand.Create();
            SignIn.Subscribe(_ => OnSignIn());
            Register = ReactiveCommand.Create();
        }

        /// <inheritdoc/>
        public IReactiveCommand<object> SignIn { get; }

        /// <inheritdoc/>
        public IReactiveCommand<object> Register { get; }

        /// <summary>
        /// Called when the <see cref="SignIn"/> command is executed.
        /// </summary>
        private void OnSignIn()
        {
            // Show the Sign In dialog. We don't need to listen to the outcome of this: the parent
            // GitHubPaneViewModel will listen to RepositoryHosts.IsLoggedInToAnyHost and close
            // this view when the user logs in.
            uiProvider.SetupUI(UIControllerFlow.Authentication, null);
            uiProvider.RunUI();
        }
    }
}