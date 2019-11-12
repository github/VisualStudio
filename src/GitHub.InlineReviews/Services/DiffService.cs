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
        readonly IGitService gitService;

        [ImportingConstructor]
        public DiffService(IGitService gitService)
        {
            this.gitService = gitService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<DiffChunk>> Diff(
            IRepository repo,
            string baseSha,
            string headSha,
            string relativePath)
        {
            var patch = await gitService.Compare(repo, baseSha, headSha, relativePath);

            if (patch != null)
            {
                return DiffUtilities.ParseFragment(patch).ToList();
            }
            else
            {
                return Array.Empty<DiffChunk>();
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
            var changes = await gitService.CompareWith(repo, baseSha, headSha, path, contents);

            if (changes?.Patch != null)
            {
                return DiffUtilities.ParseFragment(changes.Patch).ToList();
            }
            else
            {
                return Array.Empty<DiffChunk>();
            }
        }
    }
}
