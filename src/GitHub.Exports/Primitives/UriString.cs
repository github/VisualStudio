using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using GitHub.Extensions;

namespace GitHub.Primitives
{
    /// <summary>
    /// This class represents a URI given to us as a string and is implicitly 
    /// convertible to and from string.
    /// </summary>
    /// <remarks>
    /// This typically represents a URI from an external source such as user input, a 
    /// Git Repo Remote, or an API URL.  We try to preserve the original form and let 
    /// downstream clients validate the URL. This class doesn't validate the URL. It just 
    /// performs a best-effort to parse the URI into bits important to us. For example, 
    /// we need to know the HOST so we can compare against GitHub.com, GH:E instances, etc.
    /// </remarks>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "GetObjectData is implemented in the base class")]
    [Serializable]
    [TypeConverter(typeof(UriStringConverter))]
    public class UriString : StringEquivalent<UriString>, IEquatable<UriString>
    {
        static readonly Regex sshRegex = new Regex(@"^.+@(?<host>(\[.*?\]|[a-z0-9-.]+?))(:(?<owner>.*?))?(/(?<repo>.*)(\.git)?)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly Uri url;

        public UriString(string uriString) : base(NormalizePath(uriString))
        {
            if (uriString == null || uriString.Length == 0) return;
            if (Uri.TryCreate(uriString, UriKind.Absolute, out url))
            {
                if (!url.IsFile)
                    SetUri(url);
                else
                    SetFilePath(url);
            }
            else if (!ParseScpSyntax(uriString))
            {
                SetFilePath(uriString);
            }

            if (Owner != null && RepositoryName != null)
            {
                NameWithOwner = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Owner, RepositoryName);
            }
            else if (Owner != null)
            {
                NameWithOwner = Owner;
            }
            else if (RepositoryName != null)
            {
                NameWithOwner = RepositoryName;
            }
        }

        public static UriString ToUriString(Uri uri)
        {
            return uri == null ? null : new UriString(uri.ToString());
        }

        public Uri ToUri()
        {
            if (url == null)
                throw new InvalidOperationException("This Uri String is not a valid Uri");
            return url;
        }

        void SetUri(Uri uri)
        {
            var ownerSegment = FindSegment(uri.Segments, 0);
            var repositorySegment = FindSegment(uri.Segments, 1);

            Host = uri.Host;
            Owner = ownerSegment;
            RepositoryName = GetRepositoryName(repositorySegment);
            IsHypertextTransferProtocol = uri.IsHypertextTransferProtocol();

            string FindSegment(string[] segments, int number)
                => segments.Skip(number + 1).FirstOrDefault()?.TrimEnd("/");
        }

        void SetFilePath(Uri uri)
        {
            Host = "";
            Owner = "";
            RepositoryName = GetRepositoryName(uri.Segments.Last());
            IsFileUri = true;
        }

        void SetFilePath(string path)
        {
            Host = "";
            Owner = "";
            RepositoryName = GetRepositoryName(path.Replace("/", @"\").RightAfterLast(@"\"));
            IsFileUri = true;
        }

        // For xml serialization
        protected UriString()
        {
        }

        bool ParseScpSyntax(string scpString)
        {
            var match = sshRegex.Match(scpString);
            if (match.Success)
            {
                Host = match.Groups["host"].Value.ToNullIfEmpty();
                Owner = match.Groups["owner"].Value.ToNullIfEmpty();
                RepositoryName = GetRepositoryName(match.Groups["repo"].Value);
                IsScpUri = true;
                return true;
            }
            return false;
        }

        public string Host { get; private set; }

        public string Owner { get; private set; }

        public string RepositoryName { get; private set; }

        public string NameWithOwner { get; private set; }

        public bool IsFileUri { get; private set; }

        public bool IsScpUri { get; private set; }

        public bool IsValidUri => url != null;

        /// <summary>
        /// Attempts a best-effort to convert the remote origin to a GitHub Repository URL,
        /// optionally changing the owner.
        /// </summary>
        /// <param name="owner">The owner to use, if null uses <see cref="Owner"/>.</param>
        /// <returns>A converted uri, or the existing one if we can't convert it (which might be null)</returns>
        public Uri ToRepositoryUrl(string owner = null)
        {
            // we only want to process urls that represent network resources
            if (!IsScpUri && (!IsValidUri || IsFileUri)) return url;

            var scheme = url != null && IsHypertextTransferProtocol
                ? url.Scheme
                : Uri.UriSchemeHttps;

            var nameWithOwner = owner != null && RepositoryName != null ?
                string.Format(CultureInfo.InvariantCulture, "{0}/{1}", owner, RepositoryName) :
                NameWithOwner;

            return new UriBuilder
            {
                Scheme = scheme,
                Host = Host,
                Path = nameWithOwner,
                Port = url?.Port == 80
                    ? -1
                    : (url?.Port ?? -1)
            }.Uri;
        }

        /// <summary>
        /// True if the URL is HTTP or HTTPS
        /// </summary>
        public bool IsHypertextTransferProtocol { get; private set; }

        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator UriString(string value)
        {
            if (value == null) return null;

            return new UriString(value);
        }

        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator string(UriString uriString)
        {
            return uriString?.Value;
        }

        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "No.")]
        public override UriString Combine(string addition)
        {
            if (url != null)
            {
                var urlBuilder = new UriBuilder(url);
                if (!String.IsNullOrEmpty(urlBuilder.Query))
                {
                    var query = urlBuilder.Query;
                    if (query.StartsWith("?", StringComparison.Ordinal))
                    {
                        query = query.Substring(1);
                    }

                    if (!addition.StartsWith("&", StringComparison.Ordinal) && query.Length > 0)
                    {
                        addition = "&" + addition;
                    }
                    urlBuilder.Query = query + addition;
                }
                else
                {
                    var path = url.AbsolutePath;
                    if (path == "/") path = "";
                    if (!addition.StartsWith("/", StringComparison.Ordinal)) addition = "/" + addition;

                    urlBuilder.Path = path + addition;
                }
                return ToUriString(urlBuilder.Uri);
            }
            return String.Concat(Value, addition);
        }

        /// <summary>
        /// Compare repository URLs ignoring any trailing ".git" or difference in case.
        /// </summary>
        /// <returns>True if URLs reference the same repository.</returns>
        public static bool RepositoryUrlsAreEqual(UriString uri1, UriString uri2)
        {
            if (!uri1.IsHypertextTransferProtocol || !uri2.IsHypertextTransferProtocol)
            {
                // Not a repository URL
                return false;
            }

            // Normalize repository URLs
            var str1 = uri1.ToRepositoryUrl().ToString();
            var str2 = uri2.ToRepositoryUrl().ToString();
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            // Makes this look better in the debugger.
            return Value;
        }

        /// <summary>
        /// Makes a copy of the URI with the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>A new <see cref="UriString"/>.</returns>
        public UriString WithOwner(string owner) => ToUriString(ToRepositoryUrl(owner));

        protected UriString(SerializationInfo info, StreamingContext context)
            : this(GetSerializedValue(info))
        {
        }

        static string GetSerializedValue(SerializationInfo info)
        {
            // First try to get the current way it's serialized, then fall back to the older way it's serialized.
            string value;
            try
            {
                value = info.GetValue("Value", typeof(string)) as string;
            }
            catch (SerializationException)
            {
                value = info.GetValue("uriString", typeof(string)) as string;
            }

            return value;
        }

        static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/');
        }

        static string GetRepositoryName(string repositoryNameSegment)
        {
            if (String.IsNullOrEmpty(repositoryNameSegment)
                || repositoryNameSegment.Equals("/", StringComparison.Ordinal))
            {
                return null;
            }

            return repositoryNameSegment.TrimEnd('/').TrimEnd(".git");
        }

        bool IEquatable<UriString>.Equals(UriString other)
        {
            return other != null && Equals(ToString(), other.ToString());
        }
    }
}
