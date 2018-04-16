using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IForkRepositoryExecuteViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ForkRepositoryExecuteViewModel : ViewModelBase, IForkRepositoryExecuteViewModel
    {
        static readonly ILogger log = LogManager.ForContext<ForkRepositoryViewModel>();

        public ForkRepositoryExecuteViewModel()
        {
            Start = ReactiveCommand.Create();
        }

        public IRepositoryModel SourceRepository { get; private set; }

        public IRepositoryModel DestinationRepository { get; private set; }

        public ReactiveCommand<object> Start { get; }

        public string Title => Resources.ForkRepositoryTitle;

        public IObservable<object> Done => Start;

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection, IAccount account)
        {
            SourceRepository = repository;
            DestinationRepository = new RemoteRepositoryModel(
                0,
                repository.Name,
                CreateForkUri(repository.CloneUrl, account.Login),
                false,
                true,
                account,
                null);
            return Task.CompletedTask;
        }

        UriString CreateForkUri(UriString url, string login)
        {
            var original = url.ToRepositoryUrl();
            return new UriString(
                string.Format("{0}://{1}/{2}/{3}",
                    original.Scheme,
                    original.Authority,
                    login,
                    url.RepositoryName));
        }
    }
}
