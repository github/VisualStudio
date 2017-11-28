using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IGitHubDialogWindowViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class GitHubDialogWindowViewModel : ViewModelBase, IGitHubDialogWindowViewModel
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly Lazy<IConnectionManager> connectionManager;
        IDialogContentViewModel content;
        Subject<object> done = new Subject<object>();
        IDisposable subscription;

        [ImportingConstructor]
        public GitHubDialogWindowViewModel(
            IGitHubServiceProvider serviceProvider,
            Lazy<IConnectionManager> connectionManager)
        {
            this.serviceProvider = serviceProvider;
            this.connectionManager = connectionManager;
        }

        public IDialogContentViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        public IObservable<object> Done => done;

        public void Dispose()
        {
            subscription?.Dispose();
            subscription = null;
        }

        public void Start(IDialogContentViewModel viewModel)
        {
            subscription?.Dispose();
            Content = viewModel;
            subscription = viewModel.Done.Subscribe(done);
        }

        public async Task StartWithConnection<T>(T viewModel)
            where T : IDialogContentViewModel, IConnectionInitializedViewModel
        {
            var connections = await connectionManager.Value.GetLoadedConnections();
            var connection = connections.FirstOrDefault(x => x.IsLoggedIn);

            if (connection == null)
            {
                var login = CreateLoginViewModel();

                subscription = login.Done.Take(1).Subscribe(async x =>
                {
                    var newConnection = (IConnection)x;

                    if (newConnection != null)
                    {
                        await viewModel.InitializeAsync(newConnection);
                        Start(viewModel);
                    }
                    else
                    {
                       done.OnNext(null);
                    }
                });

                Content = login;
            }
            else
            {
                await viewModel.InitializeAsync(connection);
                Start(viewModel);
            }
        }

        ILoginViewModel CreateLoginViewModel()
        {
            return serviceProvider.GetService<ILoginViewModel>();
        }
    }
}
