using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
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
        readonly Lazy<CompiledQuery<Page<IssueListModel>>> issuesQuery;

        [ImportingConstructor]
        public IssueService(IGraphQLClientFactory graphqlFactory)
        {
            this.graphqlFactory = graphqlFactory;
            issuesQuery = new Lazy<CompiledQuery<Page<IssueListModel>>>(CreateIssuesQuery);
        }

        public IObservable<Page<IssueListModel>> GetIssues(IRepositoryModel repository)
        {
            return Observable.Create<Page<IssueListModel>>(async subscriber =>
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);
                var graphql = await graphqlFactory.CreateConnection(hostAddress);
                var query = issuesQuery.Value;
                var vars = new Dictionary<string, object>
                {
                    { "owner", repository.Owner },
                    { "name", repository.Name },
                    { "after", null }
                };

                while (true)
                {
                    try
                    {
                        var page = await graphql.Run(query, vars);
                        subscriber.OnNext(page);
                        if (page.HasNextPage) vars["after"] = page.EndCursor;
                        else break;
                    }
                    catch (Exception ex)
                    {
                        subscriber.OnError(ex);
                        break;
                    }
                }
            });
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
                        NodeId = issue.Id,
                        Number = issue.Number,
                        Title = issue.Title,
                        Author = issue.Author.Select(author => new ActorModel
                        {
                            AvatarUrl = issue.Author.AvatarUrl(30),
                            Login = issue.Author.Login,
                        }).SingleOrDefault(),
#pragma warning disable CS0618 // Type or member is obsolete
                        UpdatedAt = issue.UpdatedAt.Value,
#pragma warning restore CS0618 // Type or member is obsolete
                    }).ToList(),
                }).Compile();
        }
    }
}
