using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;
using ReactiveUI.Legacy;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace GitHub.App.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestAnnotationsViewModel"/>
    [Export(typeof(IPullRequestAnnotationsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestAnnotationsViewModel : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        private readonly IPullRequestSessionManager sessionManager;

        IPullRequestSession session;
        string title;
        string checkSuiteName;
        string checkRunName;
        IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> annotationsDictionary;
        IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> otherAnnotationsDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestAnnotationsViewModel"/> class.
        /// </summary>
        /// <param name="sessionManager">The pull request session manager.</param>
        [ImportingConstructor]
        public PullRequestAnnotationsViewModel(IPullRequestSessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
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

        public IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> AnnotationsDictionary
        {
            get { return annotationsDictionary; }
            private set { this.RaiseAndSetIfChanged(ref annotationsDictionary, value); }
        }

        public IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> OtherAnnotationsDictionary
        {
            get { return otherAnnotationsDictionary; }
            private set { this.RaiseAndSetIfChanged(ref otherAnnotationsDictionary, value); }
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

                var changedFileDictionary = session.PullRequest.ChangedFiles.ToDictionary(model => model.FileName);

                var annotationsLookup = checkSuiteRun.checkRun.Annotations
                    .ToLookup(annotation =>
                    {
                        var inPullRequest = false;

                        if (changedFileDictionary.TryGetValue(annotation.Path, out var pullRequestFile))
                        {
                            
                        }

                        return ValueTuple.Create(annotation.Path, inPullRequest);
                    });

                var annotationPaths = annotationsLookup
                    .Select(models => models.Key.Item1)
                    .OrderBy(s => s)
                    .ToArray();

                AnnotationsDictionary = annotationPaths
                    .Where(path => changedFileDictionary.ContainsKey(path))
                    .ToDictionary(
                        path => path,
                        path => annotationsLookup[ValueTuple.Create(path, true)]
                            .Select(annotation => new PullRequestAnnotationItemViewModel(annotation))
                            .Cast<IPullRequestAnnotationItemViewModel>()
                            .ToArray()
                        );

                OtherAnnotationsDictionary = annotationPaths
                    .Where(path => !changedFileDictionary.ContainsKey(path))
                    .ToDictionary(
                        path => path,
                        path => annotationsLookup[ValueTuple.Create(path, false)]
                            .Select(annotation => new PullRequestAnnotationItemViewModel(annotation))
                            .Cast<IPullRequestAnnotationItemViewModel>()
                            .ToArray()
                        );
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}