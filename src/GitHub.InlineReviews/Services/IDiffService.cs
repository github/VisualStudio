using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.InlineReviews.Services
{
    public interface IDiffService
    {
        IEnumerable<DiffChunk> ParseFragment(string diff);
    }
}