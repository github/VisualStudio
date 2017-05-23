using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.InlineReviews.Services
{
    public interface IDiffService
    {
        IEnumerable<DiffChunk> Diff(string before, string after, int contextLines = 3);
        IEnumerable<DiffChunk> ParseFragment(string diff);
    }
}