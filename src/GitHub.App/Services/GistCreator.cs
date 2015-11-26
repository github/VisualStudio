using System.ComponentModel.Composition;
using System.Threading.Tasks;
using NLog;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IGistCreator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GistCreator : IGistCreator
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IGitHubClient gitHubClient;

        [ImportingConstructor]
        public GistCreator(IGitHubClient gitHubClient)
        {
            this.gitHubClient = gitHubClient;
        }

        public async Task<Gist> CreateGist(string fileName, bool isPublic, string content = "", string description = "")
        {
            // No good reason to guard against optional content and description.
            Guard.ArgumentNotEmptyString(fileName, nameof(fileName));

            var newGist = new NewGist
            {
                Description = description,
                Public = isPublic
            };
            newGist.Files.Add(fileName, content);

            var createdGist = await gitHubClient.Gist.Create(newGist);
            log.Debug("Created gist here: {0}", createdGist.HtmlUrl);
            return createdGist;
        }
    }
}
