using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using GitHub.VisualStudio;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryPublishService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryPublishService : IRepositoryPublishService
    {
        readonly IGitClient gitClient;
        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public RepositoryPublishService(IServiceProvider serviceProvider, IGitClient gitClient)
        {
            this.gitClient = gitClient;
            this.serviceProvider = serviceProvider;
        }

        public IObservable<Octokit.Repository> PublishRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            IApiClient apiClient)
        {
            return Observable.Defer(() => Observable.Return(serviceProvider.GetSolution()))
                .Select(VisualStudio.Services.GetRepoFromSolution)
                .SelectMany(repo => apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                    .Select(gitHubRepo => Tuple.Create(gitHubRepo, repo)))
                .SelectMany(repos => gitClient.SetRemote(repos.Item2, "origin", null).Select(_ => repos))
                .SelectMany(repo => gitClient.Push(repo.Item2, "master", "origin").Select(_ => repo.Item1));
        }
    }
}
