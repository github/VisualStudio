using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.Models
{
    /// <summary>
    /// A collection of OAuth scopes.
    /// </summary>
    public class ScopesCollection : IReadOnlyList<string>
    {
        readonly IReadOnlyList<string> inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scopes"/> class.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        public ScopesCollection(IReadOnlyList<string> scopes)
        {
            inner = scopes;
        }

        /// <inheritdoc/>
        public string this[int index] => inner[index];

        /// <inheritdoc/>
        public int Count => inner.Count;

        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator() => inner.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();

        /// <summary>
        /// Tests if received API scopes match the required API scopes.
        /// </summary>
        /// <param name="required">The required API scopes.</param>
        /// <returns>True if all required scopes are present, otherwise false.</returns>
        public bool Matches(IReadOnlyList<string> required)
        {
            foreach (var scope in required)
            {
                var found = inner.Contains(scope);

                if (!found &&
                    (scope.StartsWith("read:", StringComparison.Ordinal) ||
                     scope.StartsWith("write:", StringComparison.Ordinal)))
                {
                    // NOTE: Scopes are actually more complex than this, for example
                    // `user` encompasses `read:user` and `user:email` but just use
                    // this simple rule for now as it works for the scopes we require.
                    var adminScope = scope
                        .Replace("read:", "admin:")
                        .Replace("write:", "admin:");
                    found = inner.Contains(adminScope);
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override string ToString() => string.Join(",", inner);
    }
}
