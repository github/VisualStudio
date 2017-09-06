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

        public async Task<IReadOnlyList<DiffChunk>> Diff(
            IRepository repo,
            string baseSha,
            string headSha,
            string path)
        {
            var patch = await gitClient.Compare(repo, baseSha, headSha, path);
            return DiffUtilities.ParseFragment(patch).ToList();
        }

        public async Task<IReadOnlyList<DiffChunk>> Diff(
            IRepository repo,
            string baseSha,
            string headSha,
            string path,
            byte[] contents)
        {
            var changes = await gitClient.CompareWith(repo, baseSha, headSha, path, contents);
            return DiffUtilities.ParseFragment(changes.Patch).ToList();
        }
    }
}
