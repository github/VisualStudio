using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;
using Rothko;
using static System.FormattableString;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IChecksService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ChecksService : IChecksService
    {
        readonly IGitClient gitClient;
        readonly IGitService gitService;
        readonly IVSGitExt gitExt;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IOperatingSystem os;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public ChecksService(
            IGitClient gitClient,
            IGitService gitService,
            IVSGitExt gitExt,
            IGraphQLClientFactory graphqlFactory,
            IOperatingSystem os,
            IUsageTracker usageTracker)
        {
            this.gitClient = gitClient;
            this.gitService = gitService;
            this.gitExt = gitExt;
            this.graphqlFactory = graphqlFactory;
            this.os = os;
            this.usageTracker = usageTracker;
        }

        static ICompiledQuery<IEnumerable<List<CheckSuiteModel>>> readCheckSuites;

        public async Task<List<CheckSuiteModel>> ReadCheckSuites(
            HostAddress address,
            string owner,
            string name,
            int pullRequestNumber)
        {
            if (readCheckSuites == null)
            {
                readCheckSuites = new Query()
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .PullRequest(Var(nameof(pullRequestNumber)))
                    .Commits(last: 1).Nodes.Select(
                        commit => commit.Commit.CheckSuites(null,null, null,null, null).AllPages()
                            .Select(suite => new CheckSuiteModel
                            {
                                Conclusion = (CheckConclusionStateEnum?) suite.Conclusion,
                                Status = (CheckStatusStateEnum) suite.Status,
                                CreatedAt = suite.CreatedAt,
                                UpdatedAt = suite.UpdatedAt,
                                CheckRuns = suite.CheckRuns(null, null, null, null, null).AllPages()
                                    .Select(run => new CheckRunModel
                                    {
                                        Conclusion = (CheckConclusionStateEnum?) run.Conclusion,
                                        Status = (CheckStatusStateEnum) run.Status,
                                        StartedAt = run.StartedAt,
                                        CompletedAt = run.CompletedAt,
                                        Annotations = run.Annotations(null, null, null, null).AllPages()
                                            .Select(annotation => new CheckRunAnnotationModel
                                            {
                                                BlobUrl = annotation.BlobUrl,
                                                StartLine = annotation.StartLine,
                                                EndLine = annotation.EndLine,
                                                Filename = annotation.Filename,
                                                Message = annotation.Message,
                                                Title = annotation.Title,
                                                WarningLevel = (CheckAnnotationLevelEnum?) annotation.WarningLevel,
                                                RawDetails = annotation.RawDetails
                                            }).ToList()
                                    }).ToList()
                            }).ToList()
                        ).Compile();
                }

            var graphql = await graphqlFactory.CreateConnection(address);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(pullRequestNumber), pullRequestNumber },
            };

            var result = await graphql.Run(readCheckSuites, vars);
            return result.FirstOrDefault();
        }
    }
}
