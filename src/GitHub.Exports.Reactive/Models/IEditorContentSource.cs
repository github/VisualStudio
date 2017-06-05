using System;
using System.Threading.Tasks;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a source of editor content for a <see cref="IPullRequestSessionFile"/>.
    /// </summary>
    public interface IEditorContentSource
    {
        /// <summary>
        /// Gets the file contents from the editor.
        /// </summary>
        /// <returns>A task returning the editor content.</returns>
        Task<byte[]> GetContent();
    }
}
