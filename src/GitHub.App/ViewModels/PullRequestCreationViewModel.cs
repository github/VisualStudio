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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GitHub.ViewModels
{
    //Add properties to interface first
    //Ex would be login tab view model
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
       // readonly IReactiveCommand<IReadOnlyList<IBranch>> loadBranchesCommand;
        //readonly ObservableAsPropertyHelper<bool> isLoading;
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

            CurrentBranch = new BranchModel("master");

            assignees = new ObservableCollection<IAccount>
            {
                new Account("User1", false, false, 0, 0, Observable.Empty<BitmapSource>()),
                new Account("User2", false, false, 0, 0, Observable.Empty<BitmapSource>()),
                new Account("User3", false, false, 0, 0, Observable.Empty<BitmapSource>()),

            };

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

        ObservableCollection<IAccount> assignees;
        public ObservableCollection<IAccount> Assignees
        {
            get { return assignees; }
            private set { this.RaiseAndSetIfChanged(ref assignees, value); }
        }

        bool loadingFailed;
        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }

    }
}
