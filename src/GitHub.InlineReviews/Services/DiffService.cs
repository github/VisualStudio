using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IDiffService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DiffService : IDiffService
    {
        readonly IGitClient gitClient;

        [ImportingConstructor]
        public DiffService(IGitClient gitClient)
        {
            this.gitClient = gitClient;
        }

        public async Task<IList<DiffChunk>> Diff(
            IRepository repo,
            string sha,
            string path,
            byte[] contents)
        {
            var changes = await gitClient.CompareWith(repo, sha, path, contents);
            return ParseFragment(changes.Patch).ToList();
        }

        public IEnumerable<DiffChunk> ParseFragment(string diff)
        {
            return DiffUtilities.ParseFragment(diff);
        }
    }
}
