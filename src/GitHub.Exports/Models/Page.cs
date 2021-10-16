using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a page in a GraphQL paged collection.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public class Page<T>
    {
        /// <summary>
        /// Gets or sets the cursor for the last item.
        /// </summary>
        public string EndCursor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are more items after this page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets the total count of items in all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the items in the page.
        /// </summary>
        public IReadOnlyList<T> Items { get; set; }
    }
}