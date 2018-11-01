using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Services
{
    /// <summary>
    /// Represents a store in which drafts of messages can be held for later recall.
    /// </summary>
    public interface IMessageDraftStore
    {
        /// <summary>
        /// Tries to get a draft.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        /// <returns>The draft data if it exists, otherwise null.</returns>
        Task<T> GetDraft<T>(string key, string secondaryKey) where T : class;

        /// <summary>
        /// Gets all drafts with the specified key.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A collection of tuples describing the secondary key and data of each draft.
        /// </returns>
        Task<IEnumerable<(string secondaryKey, T data)>> GetDrafts<T>(string key) where T : class;

        /// <summary>
        /// Updates a draft.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        /// <param name="data">The draft data.</param>
        Task UpdateDraft<T>(string key, string secondaryKey, T data) where T : class;

        /// <summary>
        /// Removes a draft from the store.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        Task DeleteDraft(string key, string secondaryKey);
    }
}