using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;
using Octokit.GraphQL;
using Serilog;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IModelService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModelService : IModelService
    {
        static readonly ILogger log = LogManager.ForContext<ModelService>();

        public const string PRPrefix = "pr";
        const string TempFilesDirectory = Info.ApplicationInfo.ApplicationName;
        const string CachedFilesDirectory = "CachedFiles";

        readonly IBlobCache hostCache;
        readonly IAvatarProvider avatarProvider;

        public ModelService(
            IApiClient apiClient,
            IBlobCache hostCache,
            IAvatarProvider avatarProvider)
        {
            this.ApiClient = apiClient;
            this.hostCache = hostCache;
            this.avatarProvider = avatarProvider;
        }

        public IApiClient ApiClient { get; }

        public IObservable<IAccount> GetCurrentUser()
        {
            return GetUserFromCache().Select(Create);
        }

        public IObservable<IAccount> GetUser(string login)
        {
            return hostCache.GetAndRefreshObject("user|" + login,
                () => ApiClient.GetUser(login).Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7))
                .Select(Create);
        }

        public IObservable<GitIgnoreItem> GetGitIgnoreTemplates()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatestFromIndex(CacheIndex.GitIgnoresPrefix, () =>
                        GetGitIgnoreTemplatesFromApi(),
                        item => { },
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromDays(7))
                )
                .Select(Create)
                .Catch<GitIgnoreItem, Exception>(e =>
                {
                    log.Error(e, "Failed to retrieve GitIgnoreTemplates");
                    return Observable.Empty<GitIgnoreItem>();
                });
        }

        public IObservable<LicenseItem> GetLicenses()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatestFromIndex(CacheIndex.LicensesPrefix, () =>
                        GetLicensesFromApi(),
                        item => { },
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromDays(7))
                )
                .Select(Create)
                .Catch<LicenseItem, Exception>(e =>
                {
                    log.Error(e, "Failed to retrieve licenses");
                    return Observable.Empty<LicenseItem>();
                });
        }

        public IObservable<IReadOnlyList<IAccount>> GetAccounts()
        {
            return Observable.Zip(
                GetUser(),
                GetUserOrganizations(),
                (user, orgs) => user.Concat(orgs))
            .ToReadOnlyList(Create);
        }

        public IObservable<RemoteRepositoryModel> GetForks(RepositoryModel repository)
        {
            return ApiClient.GetForks(repository.Owner, repository.Name)
                .Select(x => CreateRemoteRepositoryModel(x));
        }

        static RemoteRepositoryModel CreateRemoteRepositoryModel(Repository repository)
        {
            var ownerAccount = new Models.Account(repository.Owner);
            var parent = repository.Parent != null ? CreateRemoteRepositoryModel(repository.Parent) : null;
            var model = new RemoteRepositoryModel(repository.Id, repository.Name, repository.CloneUrl,
                repository.Private, repository.Fork, ownerAccount, parent, repository.DefaultBranch);

            if (parent != null)
            {
                parent.DefaultBranch.DisplayName = parent.DefaultBranch.Id;
            }

            return model;
        }

        IObservable<LicenseCacheItem> GetLicensesFromApi()
        {
            return ApiClient.GetLicenses()
                .WhereNotNull()
                .Select(LicenseCacheItem.Create);
        }

        IObservable<GitIgnoreCacheItem> GetGitIgnoreTemplatesFromApi()
        {
            return ApiClient.GetGitIgnoreTemplates()
                .WhereNotNull()
                .Select(GitIgnoreCacheItem.Create);
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUser()
        {
            return hostCache.GetAndRefreshObject("user",
                () => ApiClient.GetUser().Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7))
                .TakeLast(1)
                .ToList();
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUserOrganizations()
        {
            return GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(user.Login + "|orgs",
                    () => ApiClient.GetOrganizations().Select(AccountCacheItem.Create).ToList(),
                    TimeSpan.FromMinutes(2), TimeSpan.FromDays(7)))
                // TODO: Akavache returns the cached version followed by the fresh version if > 2
                // minutes have expired from the last request. Here we make sure the latest value is
                // returned but it's a hack. We really need a better way to cache this stuff.
                .TakeLast(1)
                .Catch<IEnumerable<AccountCacheItem>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        log.Error(e, "Retrieve user organizations failed because user is not stored in the cache");
                        return Observable.Return(Enumerable.Empty<AccountCacheItem>());
                    })
                 .Catch<IEnumerable<AccountCacheItem>, Exception>(e =>
                 {
                     log.Error(e, "Retrieve user organizations failed");
                     return Observable.Return(Enumerable.Empty<AccountCacheItem>());
                 });
        }

        public IObservable<IReadOnlyList<RemoteRepositoryModel>> GetRepositories()
        {
            return GetUserRepositories(RepositoryType.Owner)
                .TakeLast(1)
                .Concat(GetUserRepositories(RepositoryType.Member).TakeLast(1))
                .Concat(GetAllRepositoriesForAllOrganizations());
        }

        IObservable<AccountCacheItem> GetUserFromCache()
        {
            return Observable.Defer(() => hostCache.GetObject<AccountCacheItem>("user"));
        }

        public IObservable<IPullRequestModel> GetPullRequest(string owner, string name, int number)
        {
            throw new NotImplementedException();
        }

        public IObservable<RemoteRepositoryModel> GetRepository(string owner, string repo)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}/{3}", CacheIndex.RepoPrefix, user.Login, owner, repo));

            return Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.GetAndFetchLatest(
                        key,
                        () => ApiClient.GetRepository(owner, repo).Select(RepositoryCacheItem.Create))
                    .Select(Create)));
        }

        public IObservable<IPullRequestModel> CreatePullRequest(LocalRepositoryModel sourceRepository, RepositoryModel targetRepository,
            BranchModel sourceBranch, BranchModel targetBranch,
            string title, string body)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}:{2}", CacheIndex.PRPrefix, targetRepository.Owner, targetRepository.Name));

            return Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.PutAndUpdateIndex(key, () =>
                        ApiClient.CreatePullRequest(
                                new NewPullRequest(title,
                                                   string.Format(CultureInfo.InvariantCulture, "{0}:{1}", sourceRepository.Owner, sourceBranch.Name),
                                                   targetBranch.Name)
                                { Body = body },
                                targetRepository.Owner,
                                targetRepository.Name)
                            .Select(PullRequestCacheItem.Create)
                        ,
                        TimeSpan.FromMinutes(30))
                )
                .Select(Create)
            );
        }

        public IObservable<Unit> InvalidateAll()
        {
            return hostCache.InvalidateAll().ContinueAfter(() => hostCache.Vacuum());
        }

        public IObservable<string> GetFileContents(RepositoryModel repo, string commitSha, string path, string fileSha)
        {
            return Observable.Defer(() => Task.Run(async () =>
            {
                // Store cached file contents a the temp directory so they can be deleted by disk cleanup etc.
                var tempDir = Path.Combine(Path.GetTempPath(), TempFilesDirectory, CachedFilesDirectory, fileSha.Substring(0, 2));
                var tempFile = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(path) + '@' + fileSha + Path.GetExtension(path));

                if (!File.Exists(tempFile))
                {
                    var contents = await ApiClient.GetFileContents(repo.Owner, repo.Name, commitSha, path);
                    Directory.CreateDirectory(tempDir);
                    File.WriteAllBytes(tempFile, Convert.FromBase64String(contents.EncodedContent));
                }

                return Observable.Return(tempFile);
            }));
        }

        IObservable<IReadOnlyList<RemoteRepositoryModel>> GetUserRepositories(RepositoryType repositoryType)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}:repos", user.Login, repositoryType),
                    () => GetUserRepositoriesFromApi(repositoryType),
                        TimeSpan.FromMinutes(2),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create))
                .Catch<IReadOnlyList<RemoteRepositoryModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        log.Error(e,
                            "Retrieving {RepositoryType} user repositories failed because user is not stored in the cache",
                            repositoryType);
                        return Observable.Return(Array.Empty<RemoteRepositoryModel>());
                    });
        }

        IObservable<IEnumerable<RepositoryCacheItem>> GetUserRepositoriesFromApi(RepositoryType repositoryType)
        {
            return ApiClient.GetUserRepositories(repositoryType)
                .WhereNotNull()
                .Select(RepositoryCacheItem.Create)
                .ToList()
                .Catch<IEnumerable<RepositoryCacheItem>, Exception>(_ => Observable.Return(Enumerable.Empty<RepositoryCacheItem>()));
        }

        IObservable<IReadOnlyList<RemoteRepositoryModel>> GetAllRepositoriesForAllOrganizations()
        {
            return GetUserOrganizations()
                .SelectMany(org => org.ToObservable())
                .SelectMany(org => GetOrganizationRepositories(org.Login).TakeLast(1));
        }

        IObservable<IReadOnlyList<RemoteRepositoryModel>> GetOrganizationRepositories(string organization)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|repos", user.Login, organization),
                    () => ApiClient.GetRepositoriesForOrganization(organization).Select(
                        RepositoryCacheItem.Create).ToList(),
                        TimeSpan.FromMinutes(2),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create))
                .Catch<IReadOnlyList<RemoteRepositoryModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        log.Error(e, "Retrieveing {Organization} org repositories failed because user is not stored in the cache",
                            organization);
                        return Observable.Return(Array.Empty<RemoteRepositoryModel>());
                    });
        }

        public IObservable<BranchModel> GetBranches(RepositoryModel repo)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|branch", user.Login, repo.Name));

            return Observable.Defer(() => keyobs
                    .SelectMany(key => ApiClient.GetBranches(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)))
                .Select(x => new BranchModel(x.Name, repo));
        }

        static GitIgnoreItem Create(GitIgnoreCacheItem item)
        {
            return GitIgnoreItem.Create(item.Name);
        }

        static LicenseItem Create(LicenseCacheItem licenseCacheItem)
        {
            return new LicenseItem(licenseCacheItem.Key, licenseCacheItem.Name);
        }

        IAccount Create(AccountCacheItem accountCacheItem)
        {
            return new Models.Account(
                accountCacheItem.Login,
                accountCacheItem.IsUser,
                accountCacheItem.IsEnterprise,
                accountCacheItem.OwnedPrivateRepositoriesCount,
                accountCacheItem.PrivateRepositoriesInPlanCount,
                accountCacheItem.AvatarUrl,
                avatarProvider.GetAvatar(accountCacheItem));
        }

        IAccount Create(string login, string avatarUrl)
        {
            return new Models.Account(
                login,
                true,
                false,
                0,
                0,
                avatarUrl,
                avatarProvider.GetAvatar(avatarUrl));
        }

        RemoteRepositoryModel Create(RepositoryCacheItem item)
        {
            return new RemoteRepositoryModel(
                item.Id,
                item.Name,
                new UriString(item.CloneUrl),
                item.Private,
                item.Fork,
                Create(item.Owner),
                item.Parent != null ? Create(item.Parent) : null)
            {
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        GitReferenceModel Create(GitReferenceCacheItem item)
        {
            return new GitReferenceModel(item.Ref, item.Label, item.Sha, item.RepositoryCloneUrl);
        }

        IPullRequestModel Create(PullRequestCacheItem prCacheItem)
        {
            return new PullRequestModel(
                prCacheItem.Number,
                prCacheItem.Title,
                Create(prCacheItem.Author),
                prCacheItem.CreatedAt,
                prCacheItem.UpdatedAt)
            {
                Assignee = prCacheItem.Assignee != null ? Create(prCacheItem.Assignee) : null,
                Base = Create(prCacheItem.Base),
                Body = prCacheItem.Body ?? string.Empty,
                CommentCount = prCacheItem.CommentCount,
                CommitCount = prCacheItem.CommitCount,
                CreatedAt = prCacheItem.CreatedAt,
                Head = Create(prCacheItem.Head),
                State = prCacheItem.State.HasValue ?
                    prCacheItem.State.Value :
                    prCacheItem.IsOpen.Value ? PullRequestStateEnum.Open : PullRequestStateEnum.Closed,
            };
        }

        public IObservable<Unit> InsertUser(AccountCacheItem user)
        {
            return hostCache.InsertObject("user", user);
        }

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        static GitHub.Models.PullRequestReviewState FromGraphQL(Octokit.GraphQL.Model.PullRequestReviewState s)
        {
            return (GitHub.Models.PullRequestReviewState)s;
        }

        public class GitIgnoreCacheItem : CacheItem
        {
            public static GitIgnoreCacheItem Create(string ignore)
            {
                return new GitIgnoreCacheItem { Key = ignore, Name = ignore, Timestamp = DateTime.Now };
            }

            public string Name { get; set; }
        }


        public class LicenseCacheItem : CacheItem
        {
            public static LicenseCacheItem Create(LicenseMetadata licenseMetadata)
            {
                return new LicenseCacheItem { Key = licenseMetadata.Key, Name = licenseMetadata.Name, Timestamp = DateTime.Now };
            }

            public string Name { get; set; }
        }

        public class RepositoryCacheItem : CacheItem
        {
            public static RepositoryCacheItem Create(Repository apiRepository)
            {
                return new RepositoryCacheItem(apiRepository);
            }

            public RepositoryCacheItem() { }

            public RepositoryCacheItem(Repository apiRepository)
            {
                Id = apiRepository.Id;
                Name = apiRepository.Name;
                Owner = AccountCacheItem.Create(apiRepository.Owner);
                CloneUrl = apiRepository.CloneUrl;
                Private = apiRepository.Private;
                Fork = apiRepository.Fork;
                Key = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Owner.Login, Name);
                CreatedAt = apiRepository.CreatedAt;
                UpdatedAt = apiRepository.UpdatedAt;
                Timestamp = apiRepository.UpdatedAt;
                Parent = apiRepository.Parent != null ? new RepositoryCacheItem(apiRepository.Parent) : null;
            }

            public long Id { get; set; }

            public string Name { get; set; }
            public AccountCacheItem Owner { get; set; }
            public string CloneUrl { get; set; }
            public bool Private { get; set; }
            public bool Fork { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public RepositoryCacheItem Parent { get; set; }
        }

        public class PullRequestCacheItem : CacheItem
        {
            public static PullRequestCacheItem Create(PullRequest pr)
            {
                return new PullRequestCacheItem(pr);
            }

            public PullRequestCacheItem() { }

            public PullRequestCacheItem(PullRequest pr)
            {
                Title = pr.Title;
                Number = pr.Number;
                Base = new GitReferenceCacheItem
                {
                    Label = pr.Base.Label,
                    Ref = pr.Base.Ref,
                    Sha = pr.Base.Sha,
                    RepositoryCloneUrl = pr.Base.Repository.CloneUrl,
                };
                Head = new GitReferenceCacheItem
                {
                    Label = pr.Head.Label,
                    Ref = pr.Head.Ref,
                    Sha = pr.Head.Sha,
                    RepositoryCloneUrl = pr.Head.Repository?.CloneUrl
                };
                CommentCount = pr.Comments;
                CommitCount = pr.Commits;
                Author = new AccountCacheItem(pr.User);
                Assignee = pr.Assignee != null ? new AccountCacheItem(pr.Assignee) : null;
                CreatedAt = pr.CreatedAt;
                UpdatedAt = pr.UpdatedAt;
                Body = pr.Body;
                State = GetState(pr);
                IsOpen = pr.State == ItemState.Open;
                Merged = pr.Merged;
                Key = Number.ToString(CultureInfo.InvariantCulture);
                Timestamp = UpdatedAt;
            }

            public string Title { get; set; }
            public int Number { get; set; }
            public GitReferenceCacheItem Base { get; set; }
            public GitReferenceCacheItem Head { get; set; }
            public int CommentCount { get; set; }
            public int CommitCount { get; set; }
            public AccountCacheItem Author { get; set; }
            public AccountCacheItem Assignee { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public string Body { get; set; }

            // Nullable for compatibility with old caches.
            public PullRequestStateEnum? State { get; set; }

            // This fields exists only for compatibility with old caches. The State property should be used.
            public bool? IsOpen { get; set; }
            public bool? Merged { get; set; }

            static PullRequestStateEnum GetState(PullRequest pullRequest)
            {
                if (pullRequest.State == ItemState.Open)
                {
                    return PullRequestStateEnum.Open;
                }
                else if (pullRequest.Merged)
                {
                    return PullRequestStateEnum.Merged;
                }
                else
                {
                    return PullRequestStateEnum.Closed;
                }
            }
        }

        public class GitReferenceCacheItem
        {
            public string Ref { get; set; }
            public string Label { get; set; }
            public string Sha { get; set; }
            public string RepositoryCloneUrl { get; set; }
        }
    }
}
