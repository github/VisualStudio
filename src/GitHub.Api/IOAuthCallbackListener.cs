using System;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.Api
{
    /// <summary>
    /// Listens for a callback from the OAuth endpoint signalling successful login.
    /// </summary>
    public interface IOAuthCallbackListener
    {
        /// <summary>
        /// Listens for a callback with a `state` matching <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="cancel">A cancellation token.</param>
        /// <returns>The temporary code included in the callback.</returns>
        Task<string> Listen(string id, CancellationToken cancel);
    }
}
