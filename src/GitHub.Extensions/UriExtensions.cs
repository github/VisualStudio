using NullGuard;
using System;

namespace GitHub.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// Appends a relative path to the URL.
        /// </summary>
        /// <remarks>
        /// The Uri constructor for combining relative URLs have a different behavior with URLs that end with /
        /// than those that don't.
        /// </remarks>
        public static Uri Append(this Uri uri, string relativePath)
        {
            if (!uri.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
            {
                uri = new Uri(uri + "/");
            }
            return new Uri(uri, new Uri(relativePath, UriKind.Relative));
        }

        public static bool IsHypertextTransferProtocol(this Uri uri)
        {
            return uri.Scheme == "http" || uri.Scheme == "https";
        }

        public static bool IsSameHost(this Uri uri, Uri compareUri)
        {
            return uri.Host.Equals(compareUri.Host, StringComparison.OrdinalIgnoreCase);
        }

        public static Uri WithAbsolutePath(this Uri uri, string absolutePath)
        {
            absolutePath = absolutePath.EnsureStartsWith('/');

            return new Uri(uri, new Uri(absolutePath, UriKind.Relative));
        }

        [return: AllowNull]
        public static Uri ToHttps([AllowNull] this Uri uri)
        {
            if (uri == null)
                return null;

            var str = uri.ToString();
            if (str.EndsWith(".git", StringComparison.Ordinal))
                str = str.Remove(str.Length - 4);

            if (str.StartsWith("git@github.com:", StringComparison.Ordinal))
                str = str.Replace("git@github.com:", "https://github.com/");

            if (!Uri.TryCreate(str, UriKind.Absolute, out uri))
                return null;

            var uriBuilder = new UriBuilder(uri);

            uriBuilder.Scheme = Uri.UriSchemeHttps;
            // trick to keep uriBuilder from explicitly appending :80 to the HTTPS URI
            uriBuilder.Port = -1;
            return uriBuilder.Uri;
        }

        public static string ToUpperInvariantString(this Uri uri)
        {
            return uri == null ? "" : uri.ToString().ToUpperInvariant();
        }

        [return: AllowNull]
        public static string GetUser(this Uri uri)
        {
            var parts = uri.Segments;
            // only parse urls in the format domain/user/repo
            if (parts.Length < 3)
                return null;
            var u = parts[1];
            if (u == null)
                return null;
            u = u.TrimEnd('/');
            return u;
        }

        [return: AllowNull]
        public static string GetRepo(this Uri uri)
        {
            var parts = uri.Segments;
            // only parse urls in the format domain/user/repo
            if (parts.Length < 3)
                return null;
            var name = parts[2];
            if (name == null)
                return null;
            if (name.EndsWith(".git", StringComparison.Ordinal))
                name = name.Remove(name.Length - 4);
            return name;
        }
    }
}
