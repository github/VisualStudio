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
    /// <summary>
    /// Service for generating parsed diffs.
    /// </summary>
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

        /// <inheritdoc/>
        public async Task<IReadOnlyList<DiffChunk>> Diff(
            IRepository repo,
            string baseSha,
            string headSha,
            string path)
        {
            var patch = await gitClient.Compare(repo, baseSha, headSha, path);

            if (patch != null)
            {
                return DiffUtilities.ParseFragment(patch).ToList();
            }
            else
            {
                return new DiffChunk[0];
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<DiffChunk>> Diff(
            IRepository repo,
            string baseSha,
            string headSha,
            string path,
            byte[] contents)
        {
            var changes = await gitClient.CompareWith(repo, baseSha, headSha, path, contents);

            if (changes?.Patch != null)
            {
                return DiffUtilities.ParseFragment(changes.Patch).ToList();
            }
            else
            {
                return new DiffChunk[0];
            }
        }
    }
}
