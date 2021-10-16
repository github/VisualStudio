using System;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class ForkRepositorySwitchViewModelDesigner : ViewModelBase, IForkRepositorySwitchViewModel
    {
        public ForkRepositorySwitchViewModelDesigner()
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

        public string Title => null;

        public IObservable<object> Done => null;

        public IRepositoryModel SourceRepository { get; }

        public IRepositoryModel DestinationRepository { get; }

        public IReactiveCommand<object> SwitchFork => null;

        public bool ResetMasterTracking { get; set; } = true;

        public bool AddUpstream { get; set; } = true;

        public bool UpdateOrigin { get; set; } = true;

        public void Initialize(ILocalRepositoryModel sourceRepository, IRemoteRepositoryModel remoteRepository)
        {
        }
    }
}