using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    /// <summary>
    /// Base class for issue and pull request services.
    /// </summary>
    public abstract class IssueishService : IIssueishService
    {
        static ICompiledQuery<CommentModel> postComment;
        readonly IApiClientFactory apiClientFactory;
        readonly IGraphQLClientFactory graphqlFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueishService"/> class.
        /// </summary>
        /// <param name="apiClientFactory">The API client factory.</param>
        /// <param name="graphqlFactory">The GraphQL client factory.</param>
        public IssueishService(
            IApiClientFactory apiClientFactory,
            IGraphQLClientFactory graphqlFactory)
        {
            this.apiClientFactory = apiClientFactory;
            this.graphqlFactory = graphqlFactory;
        }

        /// <inheritdoc/>
        public async Task CloseIssueish(HostAddress address, string owner, string repository, int number)
        {
            var client = await apiClientFactory.CreateGitHubClient(address).ConfigureAwait(false);
            var update = new IssueUpdate { State = ItemState.Closed };
            await client.Issue.Update(owner, repository, number, update).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ReopenIssueish(HostAddress address, string owner, string repository, int number)
        {
            var client = await apiClientFactory.CreateGitHubClient(address).ConfigureAwait(false);
            var update = new IssueUpdate { State = ItemState.Open };
            await client.Issue.Update(owner, repository, number, update).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<CommentModel> PostComment(HostAddress address, string issueishId, string body)
        {
            var input = new AddCommentInput
            {
                Body = body,
                SubjectId = new ID(issueishId),
            };

            if (postComment == null)
            {
                postComment = new Mutation()
                    .AddComment(Var(nameof(input)))
                    .CommentEdge
                    .Node
                    .Select(comment => new CommentModel
                    {
                        Author = new ActorModel
                        {
                            Login = comment.Author.Login,
                            AvatarUrl = comment.Author.AvatarUrl(null),
                        },
                        Body = comment.Body,
                        CreatedAt = comment.CreatedAt,
                        DatabaseId = comment.DatabaseId.Value,
                        Id = comment.Id.Value,
                        Url = comment.Url,
                    }).Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(input), input },
            };

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            return await graphql.Run(postComment, vars).ConfigureAwait(false);
        }

        public async Task DeleteComment(
            HostAddress address,
            string owner,
            string repository,
            int commentId)
        {
            var client = await apiClientFactory.CreateGitHubClient(address).ConfigureAwait(false);
            await client.Issue.Comment.Delete(owner, repository, commentId).ConfigureAwait(false);
        }

        public async Task EditComment(
            HostAddress address,
            string owner,
            string repository,
            int commentId,
            string body)
        {
            var client = await apiClientFactory.CreateGitHubClient(address).ConfigureAwait(false);
            await client.Issue.Comment.Update(owner, repository, commentId, body).ConfigureAwait(false);
        }
    }
}
