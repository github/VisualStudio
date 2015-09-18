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
        public RepositoryPublishService(IGitClient gitClient, IVSServices services)
        {
            this.gitClient = gitClient;
            this.activeRepository = services.GetActiveRepo();
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
            return Observable.Defer(() => Observable.Return(activeRepository))
                .SelectMany(r => apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                    .Select(gitHubRepo => Tuple.Create(gitHubRepo, r)))
                    .SelectMany(repo => gitClient.SetRemote(repo.Item2, "origin", new Uri(repo.Item1.CloneUrl)).Select(_ => repo))
                    .SelectMany(repo => gitClient.Push(repo.Item2, "master", "origin").Select(_ => repo))
                    .SelectMany(repo => gitClient.Fetch(repo.Item2, "origin").Select(_ => repo))
                    .SelectMany(repo => gitClient.SetTrackingBranch(repo.Item2, "master", "origin").Select(_ => repo.Item1));
        }
    }
}
