using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
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
        readonly IRepositoryForkService repositoryForkService;

        IApiClient apiClient;

        [ImportingConstructor]
        public ForkRepositoryExecuteViewModel(
            IModelServiceFactory modelServiceFactory,
            IRepositoryForkService repositoryForkService
            )
        {
            this.modelServiceFactory = modelServiceFactory;
            this.repositoryForkService = repositoryForkService;

            this.WhenAnyValue(model => model.UpdateOrigin)
                .Subscribe(value => CanAddUpstream = value);

            this.WhenAnyValue(model => model.UpdateOrigin, model => model.AddUpstream)
                .Subscribe(tuple => CanResetMasterTracking = tuple.Item1 && tuple.Item2);

            CreateFork = ReactiveCommand.CreateFromObservable(OnCreateFork);
            BackCommand = ReactiveCommand.Create(() => { });
        }

        public RepositoryModel SourceRepository { get; private set; }

        public IAccount DestinationAccount { get; private set; }

        public RepositoryModel DestinationRepository { get; private set; }

        public ReactiveCommand<Unit, Repository> CreateFork { get; }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }

        public string Title => Resources.ForkRepositoryTitle;

        public IObservable<object> Done => CreateFork.Where(repository => repository != null);

        public IObservable<Unit> Back => BackCommand;

        public async Task InitializeAsync(LocalRepositoryModel sourceRepository, IAccount destinationAccount, IConnection connection)
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
            var forkUri = string.Format(CultureInfo.CurrentCulture, "{0}://{1}/{2}/{3}", original.Scheme, original.Authority, login, url.RepositoryName);
            return new UriString(forkUri);
        }

        IObservable<Repository> OnCreateFork()
        {
            var newRepositoryFork = new NewRepositoryFork
            {
                Organization = !DestinationAccount.IsUser ? DestinationAccount.Login : null
            };

            return repositoryForkService
                .ForkRepository(apiClient, SourceRepository, newRepositoryFork, UpdateOrigin, CanAddUpstream && AddUpstream, CanResetMasterTracking && ResetMasterTracking)
                .Catch<Repository, Exception>(ex =>
                {
                    log.Error(ex, "Error Creating Fork");

                    var apiEx = ex as ApiException;
                    Error = apiEx != null ? apiEx.Message : "An unexpected error occurred.";

                    return Observable.Return<Repository>(null);
                });
        }

        bool updateOrigin = true;
        public bool UpdateOrigin
        {
            get { return updateOrigin; }
            set { this.RaiseAndSetIfChanged(ref updateOrigin, value); }
        }

        bool canAddUpstream = true;
        public bool CanAddUpstream
        {
            get { return canAddUpstream; }
            private set { this.RaiseAndSetIfChanged(ref canAddUpstream, value); }
        }

        bool addUpstream = true;
        public bool AddUpstream
        {
            get { return addUpstream; }
            set { this.RaiseAndSetIfChanged(ref addUpstream, value); }
        }

        bool canResetMasterTracking;
        public bool CanResetMasterTracking
        {
            get { return canResetMasterTracking; }
            private set { this.RaiseAndSetIfChanged(ref canResetMasterTracking, value); }
        }

        bool resetMasterTracking;
        public bool ResetMasterTracking
        {
            get { return resetMasterTracking; }
            set { this.RaiseAndSetIfChanged(ref resetMasterTracking, value); }
        }

        string error = null;

        public string Error
        {
            get { return error; }
            private set { this.RaiseAndSetIfChanged(ref error, value); }
        }
    }
}
