using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitHub.Models;

namespace GitHub.Services
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        static readonly string[] TemplatePaths = new[]
        {
            "PULL_REQUEST_TEMPLATE.md",
            "PULL_REQUEST_TEMPLATE",
            ".github\\PULL_REQUEST_TEMPLATE.md",
            ".github\\PULL_REQUEST_TEMPLATE",
        };

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

        public string GetPullRequestTemplate(ISimpleRepositoryModel repository)
        {
            Extensions.Guard.ArgumentNotNull(repository, nameof(repository));

            var paths = TemplatePaths.Select(x => Path.Combine(repository.LocalPath, x));

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    try { return File.ReadAllText(path); } catch { }
                }
            }

            return null;
        }
    }
}