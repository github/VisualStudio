using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryCreationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryCreationService : IRepositoryCreationService
    {
        readonly ICloneService cloneService;

        [ImportingConstructor]
        public RepositoryCreationService(ICloneService cloneService)
        {
            this.cloneService = cloneService;
        }

        public IObservable<Unit> CreateRepository(
            NewRepository newRepository,
            IAccount account,
            string directory,
            IApiClient apiClient)
        {
            return apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                .Select(repository => cloneService.CloneRepository(repository.CloneUrl, repository.Name, directory))
                .SelectUnit();
        }
    }
}
