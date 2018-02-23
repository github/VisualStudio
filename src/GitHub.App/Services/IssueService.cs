using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading;
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

        public IObservable<Page<IssueListModel>> GetIssues(
            IRepositoryModel repository,
            CancellationToken cancel)
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
                        cancel.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex)
                    {
                        subscriber.OnError(ex);
                        break;
                    }
                }
            });
        }

#pragma warning disable CS0618 // Type or member is obsolete
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
                        Labels = issue.Labels(100, null, null, null).Nodes.Select(label => new IssueLabelModel
                        {
                            Color = '#' + label.Color,
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
