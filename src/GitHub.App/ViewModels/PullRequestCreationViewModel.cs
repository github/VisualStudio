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
        readonly IReactiveCommand<IReadOnlyList<IBranch>> loadBranchesCommand;
        readonly ObservableAsPropertyHelper<bool> isLoading;
        private bool IsLoading;


        [ImportingConstructor]
        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            Branches = new ReactiveList<IBranch>();
            loadBranchesCommand = ReactiveCommand.CreateAsyncObservable(loadBranches);
            isLoading = this.WhenAny(x => x.LoadingFailed, x => x.Value)
                .CombineLatest(loadBranchesCommand.IsExecuting, (failed, loading) => !failed && loading)
                .ToProperty(this, x => x.IsLoading);
            loadBranchesCommand.Subscribe(Branches.AddRange);
           
        }

        IReadOnlyList<IRepositoryModel> branches;

        public IReactiveList<IBranch> Branches { get; }

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

        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }

        readonly IBranch repositoryHost;
        private bool loadingFailed;

        IObservable<IReadOnlyList<IBranch>> loadBranches(ISimpleRepositoryModel repository)
        {
            //TODO:Need a Branch host, like RepositoryHost?
            return new ModelService.GetBranches(repository)
                .Catch<IReadOnlyList<IBranch>, Exception>(ex =>
                {
                    log.Error("Error while loading repositories", ex);
                    return Observable.Start(() => LoadingFailed = true, RxApp.MainThreadScheduler)
                        .Select(_ => new IBranch[] { });
                });
        }

    }
}
