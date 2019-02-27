using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Represents the top-level view model for the GitHub dialog.
    /// </summary>
    [Export(typeof(IGitHubDialogWindowViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class GitHubDialogWindowViewModel : ViewModelBase, IGitHubDialogWindowViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly Lazy<IConnectionManager> connectionManager;
        IDialogContentViewModel content;
        Subject<object> done = new Subject<object>();
        IDisposable subscription;

        [ImportingConstructor]
        public GitHubDialogWindowViewModel(
            IViewViewModelFactory factory,
            Lazy<IConnectionManager> connectionManager)
        {
            this.factory = factory;
            this.connectionManager = connectionManager;
        }

        /// <inheritdoc/>
        public IDialogContentViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        /// <inheritdoc/>
        public IObservable<object> Done => done;

        /// <inheritdoc/>
        public void Dispose()
        {
            subscription?.Dispose();
            subscription = null;
        }

        /// <inheritdoc/>
        public void Start(IDialogContentViewModel viewModel)
        {
            subscription?.Dispose();
            Content = viewModel;
            subscription = viewModel.Done?.Subscribe(done);
        }

        /// <inheritdoc/>
        public async Task StartWithConnection<T>(T viewModel)
            where T : IDialogContentViewModel, IConnectionInitializedViewModel
        {
            var connections = await connectionManager.Value.GetLoadedConnections().ConfigureAwait(true);
            var connection = connections.FirstOrDefault(x => x.IsLoggedIn);

            if (connection == null)
            {
                connection = await ShowLogin().ConfigureAwait(true);
            }

            if (connection != null)
            {
                await viewModel.InitializeAsync(connection).ConfigureAwait(true);
                Start(viewModel);
            }
            else
            {
                done.OnNext(null);
            }
        }

        public Task StartWithLogout<T>(T viewModel, IConnection connection)
            where T : IDialogContentViewModel, IConnectionInitializedViewModel
        {
            var logout = factory.CreateViewModel<ILogOutRequiredViewModel>();

            subscription?.Dispose();
            subscription = logout.Done.Take(1).Subscribe(async _ =>
            {
                await connectionManager.Value.LogOut(connection.HostAddress).ConfigureAwait(true);

                connection = await ShowLogin().ConfigureAwait(true);

                if (connection != null)
                {
                    await viewModel.InitializeAsync(connection).ConfigureAwait(true);
                    Start(viewModel);
                }
            });

            Content = logout;
            return Task.CompletedTask;
        }

        async Task<IConnection> ShowLogin()
        {
            var login = factory.CreateViewModel<ILoginViewModel>();
            Content = login;
            return (IConnection)await login.Done.Take(1);
        }
    }
}
