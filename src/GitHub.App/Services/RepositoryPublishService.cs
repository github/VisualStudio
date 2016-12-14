using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Helpers;
using GitHub.Models;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryPublishService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryPublishService : IRepositoryPublishService
    {
        readonly IGitClient gitClient;
        readonly IVSGitServices vsGitServices;

        [ImportingConstructor]
        public RepositoryPublishService(IGitClient gitClient, IVSGitServices vsGitServices)
        {
            this.gitClient = gitClient;
            this.vsGitServices = vsGitServices;
        }

        public string LocalRepositoryName
        {
            get
            {
                var activeRepo = vsGitServices.GetActiveRepo();
                if (!string.IsNullOrEmpty(activeRepo?.Info?.WorkingDirectory))
                    return new DirectoryInfo(activeRepo.Info.WorkingDirectory).Name ?? "";
                return string.Empty;
            }
        }

        public IObservable<Octokit.Repository> PublishRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            IApiClient apiClient)
        {
            return Observable.Defer(() => apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                             .SelectMany(async remoteRepo => new { RemoteRepo = remoteRepo, LocalRepo = await GetActiveRepo() }))
                             .SelectMany(async repo =>
                             {
                                 await gitClient.SetRemote(repo.LocalRepo, "origin", new Uri(repo.RemoteRepo.CloneUrl));
                                 await gitClient.Push(repo.LocalRepo, "master", "origin");
                                 await gitClient.Fetch(repo.LocalRepo, "origin");
                                 await gitClient.SetTrackingBranch(repo.LocalRepo, "master", "origin");
                                 return repo.RemoteRepo;
                             });
        }

        async Task<IRepository> GetActiveRepo()
        {
            await ThreadingHelper.SwitchToMainThreadAsync();
            return vsGitServices.GetActiveRepo();
        }
    }
}
