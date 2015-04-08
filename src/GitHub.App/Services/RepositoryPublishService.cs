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
        readonly IVSServices services;

        [ImportingConstructor]
        public RepositoryPublishService(IGitClient gitClient, IVSServices services)
        {
            this.gitClient = gitClient;
            this.services = services;
        }

        public IObservable<Octokit.Repository> PublishRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            IApiClient apiClient)
        {
            return Observable.Defer(() => Observable.Return(services.GetActiveRepo()))
                .SelectMany(r => apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                    .Select(gitHubRepo => Tuple.Create(gitHubRepo, r)))
                    .SelectMany(repo => gitClient.SetRemote(repo.Item2, "origin", new Uri(repo.Item1.CloneUrl)).Select(_ => repo))
                    .SelectMany(repo => gitClient.Push(repo.Item2, "master", "origin").Select(_ => repo.Item1));
        }
    }
}
