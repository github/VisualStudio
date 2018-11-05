using System;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using Octokit;
using ReactiveUI;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.SampleData
{
    public class ForkRepositoryExecuteViewModelDesigner : ViewModelBase, IForkRepositoryExecuteViewModel
    {
        public ForkRepositoryExecuteViewModelDesigner()
        {
            SourceRepository = new RemoteRepositoryModelDesigner
            {
                Owner = "github",
                Name = "VisualStudio",
                CloneUrl = "https://github.com/github/VisualStudio",
            };
            DestinationRepository = new RemoteRepositoryModelDesigner
            {
                Owner = "user",
                Name = "VisualStudio",
                CloneUrl = "https://github.com/user/VisualStudio",
            };
            DestinationAccount = new AccountDesigner();
        }

        public IObservable<object> Done => null;

        public IObservable<Unit> Back => null;

        public string Title => null;

        public RepositoryModel SourceRepository { get; set; }

        public RepositoryModel DestinationRepository { get; set; }

        public IAccount DestinationAccount { get; }

        public ReactiveCommand<Unit, Repository> CreateFork => null;

        public ReactiveCommand<Unit, Unit> BackCommand => null;

        public bool ResetMasterTracking { get; set; } = true;

        public bool AddUpstream { get; set; } = true;

        public bool UpdateOrigin { get; set; } = true;

        public bool CanAddUpstream => UpdateOrigin;

        public bool CanResetMasterTracking => UpdateOrigin && AddUpstream;

        public string Error { get; } = "I AM ERROR";

        public Task InitializeAsync(LocalRepositoryModel sourceRepository, IAccount destinationAccount, IConnection connection)
        {
            return Task.CompletedTask;
        }
    }
}