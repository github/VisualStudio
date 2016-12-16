using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using ReactiveUI;

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
                                     .ObserveOn(RxApp.MainThreadScheduler)
                                     .Select(remoteRepo => new { RemoteRepo = remoteRepo, LocalRepo = vsGitServices.GetActiveRepo() }))
                             .SelectMany(async repo =>
                             {
                                 await gitClient.SetRemote(repo.LocalRepo, "origin", new Uri(repo.RemoteRepo.CloneUrl));
                                 await gitClient.Push(repo.LocalRepo, "master", "origin");
                                 await gitClient.Fetch(repo.LocalRepo, "origin");
                                 await gitClient.SetTrackingBranch(repo.LocalRepo, "master", "origin");
                                 return repo.RemoteRepo;
                             });
        }
    }
}
