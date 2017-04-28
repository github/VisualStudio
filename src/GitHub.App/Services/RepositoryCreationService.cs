using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryCreationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryCreationService : IRepositoryCreationService
    {
        readonly IRepositoryCloneService cloneService;

        [ImportingConstructor]
        public RepositoryCreationService(IRepositoryCloneService cloneService)
        {
            this.cloneService = cloneService;
        }

        public string DefaultClonePath
        {
            get { return cloneService.DefaultClonePath; }
        }

        public IObservable<Unit> CreateRepository(
            NewRepository newRepository,
            IAccount account,
            string directory,
            IApiClient apiClient)
        {
            Guard.ArgumentNotEmptyString(directory, nameof(directory));

            return apiClient.CreateRepository(newRepository, account.Login, account.IsUser)
                .Select(repository => cloneService.CloneRepository(repository.CloneUrl, repository.Name, directory))
                .SelectUnit();
        }
    }
}
