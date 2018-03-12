using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    [Export(typeof(IIssueService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssueService : IIssueService
    {
        readonly IGraphQLClientFactory graphqlFactory;
        readonly Lazy<CompiledQuery<Page<ActorModel>>> assignableUsersQuery;
        readonly Lazy<CompiledQuery<Page<IssueListModel>>> issuesQuery;

        [ImportingConstructor]
        public IssueService(IGraphQLClientFactory graphqlFactory)
        {
            this.graphqlFactory = graphqlFactory;
            issuesQuery = new Lazy<CompiledQuery<Page<IssueListModel>>>(CreateIssuesQuery);
            assignableUsersQuery = new Lazy<CompiledQuery<Page<ActorModel>>>(CreateAssignableUsersQuery);
        }

        public async Task<Page<IssueListModel>> GetIssues(
            IRepositoryModel repository,
            string after,
            bool refresh)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl);
            var graphql = await graphqlFactory.Create(hostAddress);
            var client = refresh ? graphql.NonCached : graphql;
            var query = issuesQuery.Value;
            var vars = new Dictionary<string, object>
            {
                { "owner", repository.Owner },
                { "name", repository.Name },
                { "after", after },
            };
            return await client.Run(query, vars);
        }

        public async Task<Page<ActorModel>> GetAssignees(
            IRepositoryModel repository,
            string after)
        {
            var hostAddress = HostAddress.Create(repository.CloneUrl);
            var client = await graphqlFactory.Create(hostAddress);
            var query = assignableUsersQuery.Value;
            var vars = new Dictionary<string, object>
            {
                { "owner", repository.Owner },
                { "name", repository.Name },
                { "after", after },
            };
            return await client.Run(query, vars);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        CompiledQuery<Page<ActorModel>> CreateAssignableUsersQuery()
        {
            return new Query()
                .Repository(Var("owner"), Var("name"))
                .AssignableUsers(first: 100, after: Var("after"))
                .Select(connection => new Page<ActorModel>
                {
                    EndCursor = connection.PageInfo.EndCursor,
                    HasNextPage = connection.PageInfo.HasNextPage,
                    TotalCount = connection.TotalCount,
                    Items = connection.Nodes.Select(user => new ActorModel
                    {
                        AvatarUrl = user.AvatarUrl(30),
                        Login = user.Login,
                    }).ToList(),
                }).Compile();
        }

        CompiledQuery<Page<IssueListModel>> CreateIssuesQuery()
        {
            var order = new IssueOrder
            {
                Direction = OrderDirection.Desc,
                Field = IssueOrderField.UpdatedAt
            };

            return new Query()
                .Repository(Var("owner"), Var("name"))
                .Issues(first: 100, after: Var("after"), orderBy: order)
                .Select(connection => new Page<IssueListModel>
                {
                    EndCursor = connection.PageInfo.EndCursor,
                    HasNextPage = connection.PageInfo.HasNextPage,
                    TotalCount = connection.TotalCount,
                    Items = connection.Nodes.Select(issue => new IssueListModel
                    {
                        Author = issue.Author.Select(actor => new ActorModel
                        {
                            AvatarUrl = actor.AvatarUrl(30),
                            Login = actor.Login,
                        }).SingleOrDefault(),
                        Assignees = issue.Assignees(100, null, null, null).Nodes.Select(actor => new ActorModel
                        {
                            AvatarUrl = actor.AvatarUrl(30),
                            Login = actor.Login,
                        }).ToList(),
                        CommentCount = issue.Comments(0, null, null, null).TotalCount,
                        NodeId = issue.Id,
                        Number = issue.Number,
                        State = (Models.IssueState)issue.State,
                        Labels = issue.Labels(100, null, null, null).Nodes.Select(label => new LabelModel
                        {
                            Color = label.Color,
                            Name = label.Name,
                        }).ToList(),
                        Title = issue.Title,
                        UpdatedAt = issue.UpdatedAt.Value,
                    }).ToList(),
                }).Compile();
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
