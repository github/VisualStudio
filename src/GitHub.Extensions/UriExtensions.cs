using System;

namespace GitHub.Extensions
{
    public static class UriExtensions
    {
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

        public static Uri ToHttps(this Uri uri)
        {
            if (uri == null
                || uri.Scheme != Uri.UriSchemeHttp
                || (uri.Port != 80 && uri.Port != -1)
                || uri.IsLoopback)
                return uri;

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
    }
}
