using System;
using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        public IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, string title, string body, IBranch source, IBranch target)
        {
            Extensions.Guard.ArgumentNotNull(host, nameof(host));
            Extensions.Guard.ArgumentNotNull(repository, nameof(repository));
            Extensions.Guard.ArgumentNotNull(title, nameof(title));
            Extensions.Guard.ArgumentNotNull(body, nameof(body));
            Extensions.Guard.ArgumentNotNull(source, nameof(source));
            Extensions.Guard.ArgumentNotNull(target, nameof(target));

            return host.ModelService.CreatePullRequest(repository, title, body, source, target);
        }
    }
}