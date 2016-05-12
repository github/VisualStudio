using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using NLog;
using NullGuard;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //We don't need a reactive list
            // Branches = new ReactiveList<IBranch>();
            //loadBranchesCommand = ReactiveCommand.CreateAsyncObservable(LoadBranches);
            Branches = new ReactiveUI.ReactiveList<IBranch>
            {
               new Models.Branch { Name = "don/stub-ui" },
               new Models.Branch { Name = "feature/pr/views" },
               new Models.Branch { Name = "release-1.0.17.0" }
            };
            //isLoading = this.WhenAny(x => x.LoadingFailed, x => x.Value)
            // .CombineLatest(loadBranchesCommand.IsExecuting, (failed, loading) => !failed && loading)
            //.ToProperty(this, x => x.IsLoading);
           // loadBranchesCommand.Subscribe(Branches.AddRange);
           
        }

        IReactiveList<IBranch> branches;
        public IReactiveList<IBranch> Branches
        {
            get { return branches ; }
            private set { this.RaiseAndSetIfChanged(ref branches, value); }
        }

        public IBranch CurrentBranch { get; private set; }

        public IAccount SelectedAssignee {get; private set;}

        public IBranch TargetBranch { get; private set; }

        public IReadOnlyList<IAccount> Users
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool loadingFailed;
        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }


        IObservable<IReadOnlyList<IBranch>> LoadBranches(object value)
        {

            var list = repositoryHost.ModelService.GetBranches(repository);

            return list
                .Catch<IReadOnlyList<IBranch>, Exception>(ex =>
                {
                    log.Error("Error while loading repositories", ex);
                    return Observable.Start(() => LoadingFailed = true, RxApp.MainThreadScheduler)
                        .Select(_ => new IBranch[] { });
                });
        }

    }
}
