using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryPublishService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryPublishService : IRepositoryPublishService
    {
        readonly IGitClient gitClient;
        readonly IRepository activeRepository;

        [ImportingConstructor]
        public RepositoryPublishService(IGitClient gitClient, IVSGitServices vsGitServices)
        {
            this.gitClient = gitClient;
            this.activeRepository = vsGitServices.GetActiveRepo();
        }

        public string LocalRepositoryName
        {
            get
            {
                if (!string.IsNullOrEmpty(activeRepository?.Info?.WorkingDirectory))
                    return new DirectoryInfo(activeRepository.Info.WorkingDirectory).Name ?? "";
                return string.Empty;
            }
        }

        public IObservable<Octokit.Repository> PublishRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            IApiClient apiClient)
        {
            return Observable.Defer(() => apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                         .Select(remoteRepo => new { RemoteRepo = remoteRepo, LocalRepo = activeRepository }))
                    .SelectMany(repo => gitClient.SetRemote(repo.LocalRepo, "origin", new Uri(repo.RemoteRepo.CloneUrl)).Select(_ => repo))
                    .SelectMany(repo => gitClient.Push(repo.LocalRepo, "master", "origin").Select(_ => repo))
                    .SelectMany(repo => gitClient.Fetch(repo.LocalRepo, "origin").Select(_ => repo))
                    .SelectMany(repo => gitClient.SetTrackingBranch(repo.LocalRepo, "master", "origin").Select(_ => repo.RemoteRepo));
        }
    }
}
