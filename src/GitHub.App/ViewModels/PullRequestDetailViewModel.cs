using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Caches;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel repository;
        readonly IAvatarProvider avatarProvider;
        PullRequestState state;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        int commitCount;
        int filesChangedCount;
        IAccount author;
        DateTimeOffset createdAt;
        string body;
        int number;
        int changeCount;
        ChangedFilesView changedFilesView;

        [ImportingConstructor]
        PullRequestDetailViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice,
            IAvatarProvider avatarProvider)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo, avatarProvider)
        {
        }

        public PullRequestDetailViewModel(
            IRepositoryHost repositoryHost,
            ILocalRepositoryModel repository,
            IAvatarProvider avatarProvider)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;
            this.avatarProvider = avatarProvider;

            OpenOnGitHub = ReactiveCommand.Create();

            ToggleChangedFilesView = ReactiveCommand.Create();
            ToggleChangedFilesView.Subscribe(_ =>
            {
                ChangedFilesView = ChangedFilesView == ChangedFilesView.TreeView ?
                    ChangedFilesView.ListView : ChangedFilesView.TreeView;
            });
        }

        public PullRequestState State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        public string SourceBranchDisplayName
        {
            get { return sourceBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value); }
        }

        public string TargetBranchDisplayName
        {
            get { return targetBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value); }
        }

        public int CommitCount
        {
            get { return commitCount; }
            private set { this.RaiseAndSetIfChanged(ref commitCount, value); }
        }

        public int FilesChangedCount
        {
            get { return filesChangedCount; }
            private set { this.RaiseAndSetIfChanged(ref filesChangedCount, value); }
        }

        public int Number
        {
            get { return number; }
            private set { this.RaiseAndSetIfChanged(ref number, value); }
        }

        public IAccount Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

        public DateTimeOffset CreatedAt
        {
            get { return createdAt; }
            private set { this.RaiseAndSetIfChanged(ref createdAt, value); }
        }

        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        public int ChangedFilesCount
        {
            get { return changeCount; }
            private set { this.RaiseAndSetIfChanged(ref changeCount, value); }
        }

        public ChangedFilesView ChangedFilesView
        {
            get { return changedFilesView; }
            set { this.RaiseAndSetIfChanged(ref changedFilesView, value); }
        }

        public IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; } = new ReactiveList<IPullRequestChangeNode>();
        public IReactiveList<IPullRequestFileViewModel> ChangedFilesList { get; } = new ReactiveList<IPullRequestFileViewModel>();

        public ReactiveCommand<object> OpenOnGitHub { get; }
        public ReactiveCommand<object> ToggleChangedFilesView { get; }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            var prNumber = (int)data.Data;

            IsBusy = true;

            // TODO: Catch errors.
            Observable.CombineLatest(
                    repositoryHost.ApiClient.GetPullRequest(repository.Owner, repository.CloneUrl.RepositoryName, prNumber),
                    repositoryHost.ApiClient.GetPullRequestFiles(repository.Owner, repository.CloneUrl.RepositoryName, prNumber).ToList(),
                    (pr, files) => new { PullRequest = pr, Files = files })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Finally(() => IsBusy = false)
                .Subscribe(x => Load(x.PullRequest, x.Files));
        }

        public void Load(PullRequest pullRequest, IList<PullRequestFile> files)
        {
            State = CreatePullRequestState(pullRequest);
            SourceBranchDisplayName = GetBranchDisplayName(pullRequest.Head.Label);
            TargetBranchDisplayName = GetBranchDisplayName(pullRequest.Base.Label);
            CommitCount = pullRequest.Commits;
            FilesChangedCount = pullRequest.ChangedFiles;
            Title = pullRequest.Title;
            Number = pullRequest.Number;
            Author = new Models.Account(pullRequest.User, GetAvatar(pullRequest.User));
            CreatedAt = pullRequest.CreatedAt;
            Body = pullRequest.Body;
            ChangedFilesCount = files.Count;

            ChangedFilesTree.Clear();
            ChangedFilesList.Clear();
            ChangedFilesList.AddRange(CreateChangedFilesList(files));

            // WPF doesn't support AddRange here so iterate through the changes.
            foreach (var change in CreateChangedFilesTree(ChangedFilesList).Children)
            {
                ChangedFilesTree.Add(change);
            }
        }

        static PullRequestState CreatePullRequestState(PullRequest pullRequest)
        {
            if (pullRequest.State == ItemState.Open)
            {
                return new PullRequestState(true, "Open");
            }
            else if (pullRequest.Merged)
            {
                return new PullRequestState(false, "Merged");
            }
            else
            {
                return new PullRequestState(false, "Closed");
            }
        }

        static IEnumerable<IPullRequestFileViewModel> CreateChangedFilesList(IList<PullRequestFile> files)
        {
            return files.Select(x => new PullRequestFileViewModel(x.FileName, x.Status == "added", x.Status == "deleted"));
        }

        static IPullRequestDirectoryViewModel CreateChangedFilesTree(IEnumerable<IPullRequestFileViewModel> files)
        {
            var dirs = new Dictionary<string, PullRequestDirectoryViewModel>
            {
                { string.Empty, new PullRequestDirectoryViewModel(string.Empty) }
            };

            foreach (var file in files)
            {
                var dir = GetDirectory(file.Path, dirs);
                dir.Files.Add(file);
            }

            return dirs[string.Empty];
        }

        static PullRequestDirectoryViewModel GetDirectory(string path, Dictionary<string, PullRequestDirectoryViewModel> dirs)
        {
            PullRequestDirectoryViewModel dir;

            if (!dirs.TryGetValue(path, out dir))
            {
                var parentPath = Path.GetDirectoryName(path);
                var parentDir = GetDirectory(parentPath, dirs);

                dir = new PullRequestDirectoryViewModel(path);

                if (!parentDir.Directories.Any(x => x.DirectoryName == dir.DirectoryName))
                {
                    parentDir.Directories.Add(dir);
                    dirs.Add(path, dir);
                }
            }

            return dir;
        }

        string GetBranchDisplayName(string targetBranchLabel)
        {
            var parts = targetBranchLabel.Split(':');
            var owner = parts[0];
            return owner == repository.CloneUrl.Owner ? parts[1] : targetBranchLabel;
        }

        IObservable<BitmapSource> GetAvatar(User user)
        {
            return avatarProvider.GetAvatar(new AccountCacheItem(user))
                .Do(_ => { });
        }
    }
}
