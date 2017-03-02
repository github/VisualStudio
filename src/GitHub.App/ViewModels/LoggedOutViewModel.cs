using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Info;
using GitHub.Services;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// The view model for the "Sign in to GitHub" view in the GitHub pane.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.LoggedOut)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoggedOutViewModel : DialogViewModelBase, ILoggedOutViewModel
    {
        readonly IUIProvider uiProvider;
        readonly IVisualStudioBrowser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggedOutViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public LoggedOutViewModel(IUIProvider uiProvider, IVisualStudioBrowser browser)
        {
            this.uiProvider = uiProvider;
            this.browser = browser;
            SignIn = ReactiveCommand.Create();
            SignIn.Subscribe(_ => OnSignIn());
            Register = ReactiveCommand.Create();
            Register.Subscribe(_ => OnRegister());
        }

        /// <inheritdoc/>
        public IReactiveCommand<object> SignIn { get; }

        /// <inheritdoc/>
        public IReactiveCommand<object> Register { get; }

        /// <summary>
        /// Called when the <see cref="SignIn"/> command is executed.
        /// </summary>
        void OnSignIn()
        {
            // Show the Sign In dialog. We don't need to listen to the outcome of this: the parent
            // GitHubPaneViewModel will listen to RepositoryHosts.IsLoggedInToAnyHost and close
            // this view when the user logs in.
            uiProvider.RunInDialog(UIControllerFlow.Authentication);
        }

        /// <summary>
        /// Called when the <see cref="Register"/> command is executed.
        /// </summary>
        void OnRegister()
        {
            browser.OpenUrl(GitHubUrls.Pricing);
        }
    }
}