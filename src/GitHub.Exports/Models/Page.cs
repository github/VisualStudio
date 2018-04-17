using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Utility class to represent a page of GraphQL results together with paging information.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <remarks>
    /// This class extends <see cref="List{T}"/> and adds properties representing GraphQL paging
    /// information. It also exposes itself via the <see cref="Items"/> property in order to make
    /// writing GraphQL expressions more user-friendly.
    /// </remarks>
    public class Page<T> : List<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether a page exists after this.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets the cursor for the last item in the page.
        /// </summary>
        public string EndCursor { get; set; }

        /// <summary>
        /// Gets or sets the items in the page.
        /// </summary>
        public IList<T> Items
        {
            get { return this; }
            set { AddRange(value); }
        }
    }
}
