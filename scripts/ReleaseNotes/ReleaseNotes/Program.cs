using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace GhfvsReleaseNotes
{
    class Program
    {
        static readonly string owner = "github";
        static readonly string repositoryName = "VisualStudio";

        static readonly string[] Labels = IssueModel.InterestingLabels;

        public static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("GitHub for Visual Studio Release Notes Generator");
                Console.WriteLine("usage: GhfvsReleaseNotes.exe [apitoken]");
                return;
            }

            var token = args[0];
            var header = new ProductHeaderValue("Octokit.GraphQL", "0.1");
            var connection = new Connection(header, token);
            var lastRelease = await GetLastReleaseDate(connection);
            var issues = await GetIssues(connection, lastRelease);
            var prs = await GetPullRequests(connection, lastRelease);
            var all = issues.Concat(prs)
                .OrderBy(x => x.Number)
                .GroupBy(x => x.SortOrder)
                .OrderBy(x => x.Key)
                .ToList();

            using (var md = new StringWriter())
            using (var html = new StringWriter())
            {
                html.WriteLine("                  <ul class=\"changes\">");

                foreach (var section in all)
                {
                    md.WriteLine();
                    md.WriteLine($"## {IssueModel.Sections[section.Key]}");
                    md.WriteLine();

                    foreach (var i in section)
                    {
                        md.WriteLine($"#{i.Number} - {i.Title}");
                        html.WriteLine($@"                      <li>
                          <div class=""change-label-container"">
                            <em class=""change-label change-{i.Badge.ToLowerInvariant()}"">{i.Badge}</em>
                          </div>
                          <a href=""https://github.com/github/VisualStudio/issues/{i.Number}"" class=""issue-link"" title=""{i.Title}"">#{i.Number}</a> {i.Title}
                      </li>");
                    }
                }

                html.WriteLine("                  </ul>");

                var body = $"{md}\n```html\n{html}\n```\n";
                Console.WriteLine(body);

                var issue = await UpdateIssueWithLabel(token, owner, repositoryName, "release notes", body);
                Console.WriteLine(issue != null ? $"Updated issue #{issue.Number}" : "Couldn't find issue to update");
            }
        }

        static async Task<DateTimeOffset> GetLastReleaseDate(Connection connection)
        {
            var query = new Query()
                .Repository(owner: owner, name: repositoryName)
                .Releases(last: 1).Nodes
                .Select(x => x.PublishedAt);
            var result = await connection.Run(query);
            return result.Single().Value;
        }

        static async Task<IList<IssueModel>> GetIssues(Connection connection, DateTimeOffset after)
        {
            string cursor = null;
            var result = new List<IssueModel>();

            var order = new IssueOrder
            {
                Field = IssueOrderField.CreatedAt,
                Direction = OrderDirection.Asc,
            };

            do
            {
                var query = new Query()
                    .Repository(owner: owner, name: repositoryName)
                    .Issues(first: 30, after: cursor, labels: Labels, states: new[] { IssueState.Closed })
                    .Select(x => new
                    {
                        x.PageInfo.EndCursor,
                        x.PageInfo.HasNextPage,
                        Issues = x.Nodes.Select(y => new IssueModel
                        {
                            Number = y.Number,
                            Title = y.Title,
                            ClosedAt = y.ClosedAt,
                            Labels = y.Labels(30, null, null, null).Nodes.Select(z => z.Name).ToList(),
                        }).ToList(),
                    });

                var page = await connection.Run(query);

                foreach (var issue in page.Issues)
                {
                    if (issue.ClosedAt > after)
                    {
                        result.Add(issue);
                    }
                }

                if (page.HasNextPage)
                {
                    cursor = page.EndCursor;
                }
                else
                {
                    return result;
                }
            } while (true);
        }

        static async Task<IList<IssueModel>> GetPullRequests(Connection connection, DateTimeOffset after)
        {
            string cursor = null;
            var result = new List<IssueModel>();

            do
            {
                var query = new Query()
                    .Repository(owner: owner, name: repositoryName)
                    .PullRequests(first: 30, after: cursor, labels: Labels, states: new[] { PullRequestState.Merged })
                    .Select(x => new
                    {
                        x.PageInfo.EndCursor,
                        x.PageInfo.HasNextPage,
                        Issues = x.Nodes.Select(y => new IssueModel
                        {
                            Number = y.Number,
                            Title = y.Title,
                            ClosedAt = y.MergedAt,
                            Labels = y.Labels(30, null, null, null).Nodes.Select(z => z.Name).ToList(),
                        }).ToList(),
                    });

                var page = await connection.Run(query);

                foreach (var issue in page.Issues)
                {
                    if (issue.ClosedAt > after)
                    {
                        result.Add(issue);
                    }
                }

                if (page.HasNextPage)
                {
                    cursor = page.EndCursor;
                }
                else
                {
                    return result;
                }
            } while (true);
        }

        static async Task<Octokit.Issue> UpdateIssueWithLabel(
            string token, string owner, string repositoryName, string label, string body)
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("ReleaseNotes"))
            {
                Credentials = new Octokit.Credentials(token)
            };

            var issues = await client.Issue.GetAllForRepository(owner, repositoryName);

            var issue = issues
                .Where(i => i.Labels.Any(l => string.Equals(l.Name, label, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();
            if (issue == null)
            {
                return null;
            }

            var issueUpdate = new Octokit.IssueUpdate
            {
                Body = body
            };

            return await client.Issue.Update(owner, repositoryName, issue.Number, issueUpdate);
        }
    }
}
