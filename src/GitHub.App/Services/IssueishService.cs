using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace GitHub.Services
{
    public abstract class IssueishService : IIssueishService
    {
        static ICompiledQuery<CommentModel> postComment;
        readonly IGraphQLClientFactory graphqlFactory;

        public IssueishService(IGraphQLClientFactory graphqlFactory)
        {
            this.graphqlFactory = graphqlFactory;
        }

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
    }
}
