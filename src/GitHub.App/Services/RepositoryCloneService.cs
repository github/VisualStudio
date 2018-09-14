using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio.Shell;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Rothko;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories. It wraps the
    /// <see cref="Microsoft.TeamFoundation.Git.Controls.Extensibility.IGitRepositoriesExt"/> service provided
    /// by Team Explorer.
    /// </summary>
    [Export(typeof(IRepositoryCloneService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneService : IRepositoryCloneService
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryCloneService>();

        readonly IOperatingSystem operatingSystem;
        readonly string defaultClonePath;
        readonly IVSGitServices vsGitServices;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IUsageTracker usageTracker;
        ICompiledQuery<ViewerRepositoriesModel> readViewerRepositories;

        [ImportingConstructor]
        public RepositoryCloneService(
            IOperatingSystem operatingSystem,
            IVSGitServices vsGitServices,
            IGraphQLClientFactory graphqlFactory,
            IUsageTracker usageTracker)
        {
            this.operatingSystem = operatingSystem;
            this.vsGitServices = vsGitServices;
            this.graphqlFactory = graphqlFactory;
            this.usageTracker = usageTracker;

            defaultClonePath = GetLocalClonePathFromGitProvider(operatingSystem.Environment.GetUserRepositoriesPath());
        }

        /// <inheritdoc/>
        public async Task<ViewerRepositoriesModel> ReadViewerRepositories(HostAddress address)
        {
            if (readViewerRepositories == null)
            {
                var order = new RepositoryOrder
                {
                    Field = RepositoryOrderField.Name,
                    Direction = OrderDirection.Asc
                };

                var affiliation = new RepositoryAffiliation?[]
                {
                    RepositoryAffiliation.Owner, RepositoryAffiliation.Collaborator
                };

                var repositorySelection = new Fragment<Repository, RepositoryListItemModel>(
                    "repository",
                    repo => new RepositoryListItemModel
                    {
                        IsFork = repo.IsFork,
                        IsPrivate = repo.IsPrivate,
                        Name = repo.Name,
                        Owner = repo.Owner.Login,
                        Url = new Uri(repo.Url),
                    });

                readViewerRepositories = new Query()
                    .Viewer
                    .Select(viewer => new ViewerRepositoriesModel
                    {
                        Repositories = viewer.Repositories(null, null, null, null, null, order, affiliation, null, null)
                            .AllPages()
                            .Select(repositorySelection).ToList(),
                        OrganizationRepositories = viewer.Organizations(null, null, null, null).AllPages().Select(org => new
                        {
                            org.Login,
                            Repositories = org.Repositories(null, null, null, null, null, order, null, null, null)
                                .AllPages()
                                .Select(repositorySelection).ToList()
                        }).ToDictionary(x => x.Login, x => (IReadOnlyList<RepositoryListItemModel>)x.Repositories),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var result = await graphql.Run(readViewerRepositories).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task CloneRepository(
            string cloneUrl,
            string repositoryPath,
            object progress = null)
        {
            Guard.ArgumentNotEmptyString(cloneUrl, nameof(cloneUrl));
            Guard.ArgumentNotEmptyString(repositoryPath, nameof(repositoryPath));

            // Switch to a thread pool thread for IO then back to the main thread to call
            // vsGitServices.Clone() as this must be called on the main thread.
            await ThreadingHelper.SwitchToPoolThreadAsync();
            operatingSystem.Directory.CreateDirectory(repositoryPath);
            await ThreadingHelper.SwitchToMainThreadAsync();

            try
            {
                await vsGitServices.Clone(cloneUrl, repositoryPath, true, progress);
                await usageTracker.IncrementCounter(x => x.NumberOfClones);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not clone {CloneUrl} to {Path}", cloneUrl, repositoryPath);
                throw;
            }
        }

        /// <inheritdoc/>
        public bool DestinationExists(string path) => Directory.Exists(path) || File.Exists(path);

        string GetLocalClonePathFromGitProvider(string fallbackPath)
        {
            var ret = vsGitServices.GetLocalClonePathFromGitProvider();
            return !string.IsNullOrEmpty(ret)
                ? operatingSystem.Environment.ExpandEnvironmentVariables(ret)
                : fallbackPath;
        }

        public string DefaultClonePath { get { return defaultClonePath; } }

        class OrganizationAdapter
        {
            public IReadOnlyList<RepositoryListItemModel> Repositories { get; set; }
        }
    }
}
