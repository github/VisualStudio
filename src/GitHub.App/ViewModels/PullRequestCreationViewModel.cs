using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NLog;
using NullGuard;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IRepositoryHost repositoryHost;
        readonly ISimpleRepositoryModel repository;

        [ImportingConstructor]
        PullRequestCreationViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        { }

        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;

            branches = repositoryHost.ModelService.GetBranches(repository)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.Branches, initialValue: new IBranchModel[] { });

            CurrentBranch = new BranchModel { Name = repository.CurrentBranchName };

            assignees = repositoryHost.ModelService.GetAvailableAssignees(repository)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.Assignees, initialValue: new IAccount[] { });

            CreatePullRequest = InitializeCreatePullRequestCommand();

        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);
        }

        readonly ObservableAsPropertyHelper<IReadOnlyList<IBranchModel>> branches;
        public IReadOnlyList<IBranchModel> Branches
        {
            get { return branches.Value; }
        }

        public IBranchModel CurrentBranch { get; private set; }

        public IAccount SelectedAssignee {get; private set;}

        public IBranchModel TargetBranch { get; private set; }

        ObservableAsPropertyHelper<IReadOnlyList<IAccount>> assignees;
        public IReadOnlyList<IAccount> Assignees
        {
            get { return assignees.Value; }
        }

        bool loadingFailed;
        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }

        public IReactiveCommand<Unit> CreatePullRequest { get; private set; }

        //public ICommand Create => CreateCommand;
        IObservable<Unit> OnCreatePullRequest(object state)
        {
            //var newRepository = GatherRepositoryInfo();

            //return repositoryCreationService.CreateRepository(
            //    newRepository,
            //    SelectedAccount,
            //    BaseRepositoryPath,
            //    repositoryHost.ApiClient);

            return null;
        }


        ReactiveCommand<Unit> InitializeCreatePullRequestCommand()
        {
       
            var createCommand = ReactiveCommand.CreateAsyncObservable(OnCreatePullRequest);
            createCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (!Extensions.ExceptionExtensions.IsCriticalException(ex))
                {
                    log.Error("Error creating pull request.", ex);
                }
            });

            return createCommand;
        }

    }
}
