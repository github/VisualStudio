using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
                             .Select(async repo =>
                             {
                                 await gitClient.SetRemote(repo.LocalRepo, "origin", new Uri(repo.RemoteRepo.CloneUrl));
                                 await gitClient.Push(repo.LocalRepo, "master", "origin");
                                 await gitClient.Fetch(repo.LocalRepo, "origin");
                                 await gitClient.SetTrackingBranch(repo.LocalRepo, "master", "origin");
                                 return repo.RemoteRepo;
                             })
                            .Select(t => t.Result);
        }
    }
}
