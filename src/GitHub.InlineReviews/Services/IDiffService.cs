using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    public interface IDiffService
    {
        Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string relativePath);
        Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string relativePath, byte[] contents);
    }
}