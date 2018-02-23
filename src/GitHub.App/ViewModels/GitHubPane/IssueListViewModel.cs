using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IIssueListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueListViewModel : PanePageViewModelBase, IIssueListViewModel
    {
        readonly IIssueService service;

        [ImportingConstructor]
        public IssueListViewModel(IIssueService service)
        {
            this.service = service;
            Issues = new TrackingCollection<IIssueListItemViewModel>();
        }

        public ITrackingCollection<IIssueListItemViewModel> Issues { get; }
        public ILocalRepositoryModel LocalRepository { get; private set; }
        public string SearchQuery { get; set; }
        public Uri WebUrl => null;

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            LocalRepository = repository;
            Refresh();
            return Task.CompletedTask;
        }

        public override Task Refresh()
        {
            IsBusy = true;

            var sw = new Stopwatch();
            sw.Start();
            Issues.Listen(ReadItems());
            Issues.OriginalCompleted.Subscribe(_ =>
            {
                sw.Stop();
                Debug.WriteLine("Loaded issues in " + sw.Elapsed);
                IsBusy = false;
            });
            Issues.Subscribe();
            return Task.CompletedTask;
        }

        IObservable<IIssueListItemViewModel> ReadItems()
        {
            return service.GetIssues(LocalRepository)
                .SelectMany(page => page.Items.Select(x => new IssueListItemViewModel(x)));
        }
    }
}
