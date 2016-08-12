using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for the GitHub pane.
    /// </summary>
    [Export(typeof(GitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class GitHubPaneViewModel : NavigatingViewModel<IGitHubPanePage>, IDisposable
    {
        readonly IServiceProvider serviceProvider;
        readonly ITeamExplorerServiceHolder holder;
        readonly ObservableAsPropertyHelper<ReactiveCommand<object>> refresh;
        ISimpleRepositoryModel activeRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubPaneViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public GitHubPaneViewModel(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ITeamExplorerServiceHolder holder)
        {
            this.serviceProvider = serviceProvider;
            this.holder = holder;
            holder.Subscribe(this, x => ActiveRepo = x);

            refresh = this.WhenAnyValue(x => x.Content.Refresh).ToProperty(this, x => x.Refresh);
            this.WhenAnyValue(x => x.ActiveRepo).Subscribe(RepositoryChanged);
        }

        /// <summary>
        /// Gets the active repository.
        /// </summary>
        public ISimpleRepositoryModel ActiveRepo
        {
            get { return activeRepo; }
            private set { this.RaiseAndSetIfChanged(ref activeRepo, value); }
        }

        /// <summary>
        /// Gets a title to display at the top of the pane.
        /// </summary>
        public string Title => "GitHub";

        /// <summary>
        /// Gets a command used to refresh the current page.
        /// </summary>
        public ReactiveCommand<object> Refresh => refresh.Value;

        /// <summary>
        /// Disposes any resources held by the view model.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            holder.Unsubscribe(this);
        }

        /// <summary>
        /// Called when <see cref="ActiveRepo"/> changes.
        /// </summary>
        /// <param name="repo">The new active repo.</param>
        void RepositoryChanged(ISimpleRepositoryModel repo)
        {
            Clear();

            var vm = serviceProvider.GetExportedValue<INotAGitRepositoryViewModel>();
            NavigateTo(vm);
        }
    }
}
