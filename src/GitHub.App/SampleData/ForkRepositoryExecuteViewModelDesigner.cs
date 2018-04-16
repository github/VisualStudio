using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

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
        }

        public IObservable<object> Done => null;

        public string Title => null;

        public IRepositoryModel SourceRepository { get; set; }

        public IRepositoryModel DestinationRepository { get; set; }

        public ReactiveCommand<object> Start => null;

        public bool ResetMasterTracking { get; set; } = true;

        public bool AddUpstream { get; set; } = true;

        public bool UpdateOrigin { get; set; } = true;

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection, IAccount account)
        {
            throw new NotImplementedException();
        }
    }
}