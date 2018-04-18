using System;
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

        public string Title => null;

        public IRepositoryModel SourceRepository { get; set; }

        public IRepositoryModel DestinationRepository { get; set; }

        public IAccount DestinationAccount { get; }

        public IReactiveCommand<Repository> CreateFork => null;

        public bool ResetMasterTracking { get; set; } = true;

        public bool AddUpstream { get; set; } = true;

        public bool UpdateOrigin { get; set; } = true;

        public Task InitializeAsync(ILocalRepositoryModel sourceRepository, IConnection connection)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(ILocalRepositoryModel sourceRepository, IAccount destinationAccount, IConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}