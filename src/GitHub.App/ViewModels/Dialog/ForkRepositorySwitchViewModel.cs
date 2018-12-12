using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositorySwitchViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositorySwitchViewModel : ViewModelBase, IForkRepositorySwitchViewModel
    {
        readonly IRepositoryForkService repositoryForkService;

        [ImportingConstructor]
        public ForkRepositorySwitchViewModel(IRepositoryForkService repositoryForkService)
        {
            this.repositoryForkService = repositoryForkService;

            SwitchFork = ReactiveCommand.CreateFromObservable(OnSwitchFork);
        }

        public RepositoryModel SourceRepository { get; private set; }

        public RepositoryModel DestinationRepository { get; private set; }

        public ReactiveCommand<Unit, Unit> SwitchFork { get; }

        public string Title => Resources.SwitchOriginTitle;

        public IObservable<object> Done => SwitchFork.Where(value => value != null).SelectNull();

        public void Initialize(LocalRepositoryModel sourceRepository, RemoteRepositoryModel remoteRepository)
        {
            SourceRepository = sourceRepository;
            DestinationRepository = remoteRepository;
        }

        IObservable<Unit> OnSwitchFork()
        {
            return repositoryForkService.SwitchRemotes(DestinationRepository, UpdateOrigin, AddUpstream, ResetMasterTracking);
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
