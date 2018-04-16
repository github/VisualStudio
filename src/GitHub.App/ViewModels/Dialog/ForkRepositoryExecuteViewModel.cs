using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
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

        public ForkRepositoryExecuteViewModel()
        {
        }

        public IRepositoryModel SourceRepository { get; private set; }

        public IAccount DestinationAccount { get; private set; }
        public IRepositoryModel DestinationRepository { get; private set; }

        public IReactiveCommand<Repository> CreateFork { get; }

        public string Title => Resources.ForkRepositoryTitle;

        public IObservable<object> Done => CreateFork.Where(repository => repository != null);

        public Task InitializeAsync(ILocalRepositoryModel sourceRepository, IAccount destinationAccount, IConnection connection)
        {
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
            return Task.CompletedTask;
        }

        UriString CreateForkUri(UriString url, string login)
        {
            var original = url.ToRepositoryUrl();
            return new UriString($"{original.Scheme}://{original.Authority}/{login}/{url.RepositoryName}");
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
