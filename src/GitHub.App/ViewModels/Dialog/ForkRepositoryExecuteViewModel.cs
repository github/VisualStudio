using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using Serilog;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositoryExecuteViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositoryExecuteViewModel : ViewModelBase, IForkRepositoryExecuteViewModel
    {
        static readonly ILogger log = LogManager.ForContext<ForkRepositoryExecuteViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        readonly INotificationService notificationService;
        readonly IRepositoryForkService repositoryForkService;

        IApiClient apiClient;

        [ImportingConstructor]
        public ForkRepositoryExecuteViewModel(
            IModelServiceFactory modelServiceFactory,
            INotificationService notificationService,
            IRepositoryForkService repositoryForkService
            )
        {
            this.modelServiceFactory = modelServiceFactory;
            this.notificationService = notificationService;
            this.repositoryForkService = repositoryForkService;

            CreateFork = ReactiveCommand.CreateAsyncObservable(OnCreateFork);
        }

        public IRepositoryModel SourceRepository { get; private set; }

        public IAccount DestinationAccount { get; private set; }
        public IRepositoryModel DestinationRepository { get; private set; }

        public IReactiveCommand<Repository> CreateFork { get; }

        public string Title => Resources.ForkRepositoryTitle;

        public IObservable<object> Done => CreateFork.Where(repository => repository != null);

        public async Task InitializeAsync(ILocalRepositoryModel sourceRepository, IAccount destinationAccount, IConnection connection)
        {
            var modelService = await modelServiceFactory.CreateAsync(connection);
            apiClient = modelService.ApiClient;

            DestinationAccount = destinationAccount;

            SourceRepository = sourceRepository;
            DestinationRepository = new RemoteRepositoryModel(
                0,
                sourceRepository.Name,
                CreateForkUri(sourceRepository.CloneUrl, destinationAccount.Login),
                false,
                true,
                destinationAccount,
                null);
        }

        UriString CreateForkUri(UriString url, string login)
        {
            var original = url.ToRepositoryUrl();
            return new UriString($"{original.Scheme}://{original.Authority}/{login}/{url.RepositoryName}");
        }

        IObservable<Repository> OnCreateFork(object o)
        {
            var newRepositoryFork = new NewRepositoryFork
            {
                Organization = !DestinationAccount.IsUser ? DestinationRepository.Name : null
            };

            IRepositoryModel sourceRepository = SourceRepository;
            return repositoryForkService.ForkRepository(apiClient, sourceRepository, newRepositoryFork, ResetMasterTracking, AddUpstream, UpdateOrigin)
                .Catch<Repository, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex, "Error Creating Gist");
                        var error = StandardUserErrors.GetUserFriendlyErrorMessage(ex, ErrorType.RepoForkFailed);
                        notificationService.ShowError(error);
                    }

                    return Observable.Return<Repository>(null);
                });
        }

        bool resetMasterTracking = true;
        public bool ResetMasterTracking
        {
            get { return resetMasterTracking; }
            set { this.RaiseAndSetIfChanged(ref resetMasterTracking, value); }
        }

        bool addUpstream = true;
        public bool AddUpstream
        {
            get { return addUpstream; }
            set { this.RaiseAndSetIfChanged(ref addUpstream, value); }
        }

        bool updateOrigin = true;
        public bool UpdateOrigin
        {
            get { return updateOrigin; }
            set { this.RaiseAndSetIfChanged(ref updateOrigin, value); }
        }
    }
}
