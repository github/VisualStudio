using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestAnnotationsViewModel"/>
    [Export(typeof(IPullRequestAnnotationsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestAnnotationsViewModel : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        readonly IPullRequestSessionManager sessionManager;
        readonly IPullRequestEditorService pullRequestEditorService;
        readonly IUsageTracker usageTracker;

        IPullRequestSession session;
        string title;
        string checkSuiteName;
        string checkRunName;
        IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> annotationsDictionary;
        string checkRunSummary;
        string checkRunText;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestAnnotationsViewModel"/> class.
        /// </summary>
        /// <param name="sessionManager">The pull request session manager.</param>
        /// <param name="pullRequestEditorService">The pull request editor service.</param>
        [ImportingConstructor]
        public PullRequestAnnotationsViewModel(
            IPullRequestSessionManager sessionManager,
            IPullRequestEditorService pullRequestEditorService,
            IUsageTracker usageTracker)
        {
            this.sessionManager = sessionManager;
            this.pullRequestEditorService = pullRequestEditorService;
            this.usageTracker = usageTracker;

            NavigateToPullRequest = ReactiveCommand.Create(() => {
                    NavigateTo(FormattableString.Invariant(
                        $"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{PullRequestNumber}"));
                });
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(LocalRepositoryModel localRepository, IConnection connection, string owner,
            string repo, int pullRequestNumber, string checkRunId)
        {
            if (repo != localRepository.Name)
            {
                throw new NotSupportedException();
            }

            IsLoading = true;

            try
            {
                LocalRepository = localRepository;
                RemoteRepositoryOwner = owner;
                PullRequestNumber = pullRequestNumber;
                CheckRunId = checkRunId;
                session = await sessionManager.GetSession(owner, repo, pullRequestNumber);
                Load(session.PullRequest);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int PullRequestNumber { get; private set; }

        /// <inheritdoc/>
        public string CheckRunId { get; private set; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; private set; }

        /// <inheritdoc/>
        public string PullRequestTitle
        {
            get { return title; }
            private set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public string CheckSuiteName
        {
            get { return checkSuiteName; }
            private set { this.RaiseAndSetIfChanged(ref checkSuiteName, value); }
        }

        /// <inheritdoc/>
        public string CheckRunName
        {
            get { return checkRunName; }
            private set { this.RaiseAndSetIfChanged(ref checkRunName, value); }
        }

        /// <inheritdoc/>
        public string CheckRunSummary
        {
            get { return checkRunSummary; }
            private set { this.RaiseAndSetIfChanged(ref checkRunSummary, value); }
        }

        /// <inheritdoc/>
        public string CheckRunText
        {
            get { return checkRunText; }
            private set { this.RaiseAndSetIfChanged(ref checkRunText, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> AnnotationsDictionary
        {
            get { return annotationsDictionary; }
            private set { this.RaiseAndSetIfChanged(ref annotationsDictionary, value); }
        }

        void Load(PullRequestDetailModel pullRequest)
        {
            IsBusy = true;

            try
            {
                PullRequestTitle = pullRequest.Title;

                var checkSuiteRun = pullRequest
                    .CheckSuites.SelectMany(checkSuite => checkSuite.CheckRuns
                            .Select(checkRun => new{checkSuite, checkRun}))
                    .First(arg => arg.checkRun.Id == CheckRunId);

                CheckSuiteName = checkSuiteRun.checkSuite.ApplicationName;
                CheckRunName = checkSuiteRun.checkRun.Name;
                CheckRunSummary = checkSuiteRun.checkRun.Summary;
                CheckRunText = checkSuiteRun.checkRun.Text;

                var changedFiles = new HashSet<string>(session.PullRequest.ChangedFiles.Select(model => model.FileName));

                var annotationsLookup = checkSuiteRun.checkRun.Annotations
                    .ToLookup(annotation => annotation.Path);

                AnnotationsDictionary = annotationsLookup
                    .Select(models => models.Key)
                    .OrderBy(s => s)
                    .ToDictionary(
                        path => path,
                        path => annotationsLookup[path]
                            .Select(annotation => new PullRequestAnnotationItemViewModel(annotation, changedFiles.Contains(path), checkSuiteRun.checkSuite, session, pullRequestEditorService))
                            .Cast<IPullRequestAnnotationItemViewModel>()
                            .ToArray()
                        );

                usageTracker.IncrementCounter(x => x.NumberOfPullRequestOpenAnnotationsList).Forget();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}