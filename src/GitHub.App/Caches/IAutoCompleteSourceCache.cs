using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Cache
{
    public interface IAutoCompleteSourceCache
    {
        /// <summary>
        /// Retrieves suggestions from the cache for the specified repository. If not there, it makes an API
        /// call to  retrieve them.
        /// </summary>
        /// <param name="repositoryModel">The repository that contains the users</param>
        /// <returns>An observable containing a readonly list of issue suggestions</returns>
        IObservable<IReadOnlyList<SuggestionItem>> RetrieveSuggestions(IRepositoryModel repositoryModel);
    }
}
