using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class UriString : StringEquivalent<UriString>, IEquatable<UriString>
    {
        //static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        static readonly Regex sshRegex = new Regex(@"^.+@(?<host>(\[.*?\]|[a-z0-9-.]+?))(:(?<owner>.*?))?(/(?<repo>.*)(\.git)?)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly Uri url;

        public UriString(string uriString) : base(NormalizePath(uriString))
        {
            if (uriString.Length == 0) return;
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

            if (RepositoryName != null)
            {
                NameWithOwner = Owner != null 
                    ? string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Owner, RepositoryName) 
                    : RepositoryName;
            }
        }

        void SetUri(Uri uri)
        {
            Host = uri.Host;
            if (uri.Segments.Any())
            {
                RepositoryName = GetRepositoryName(uri.Segments.Last());
            }
            
            if (uri.Segments.Length > 2)
            {
                Owner = (uri.Segments[uri.Segments.Length - 2] ?? "").TrimEnd('/').ToNullIfEmpty();
            }
            
            IsHypertextTransferProtocol = uri.IsHypertextTransferProtocol();

            if (String.IsNullOrEmpty(uri.Query)) return;
            
            try
            {
                var query = ParseQueryString(uri);
                    
                if (query.ContainsKey("branch") && !String.IsNullOrEmpty(query["branch"]))
                {
                    Branch = query["branch"].Replace("%2F", "/");
                }

                if (query.ContainsKey("pr") && !String.IsNullOrEmpty(query["pr"]))
                {
                    PullRequest = query["pr"].Replace("%2F", "/");
                }

                if (query.ContainsKey("filepath") && !String.IsNullOrEmpty(query["filepath"]))
                {
                    RelativePathToOpen = query["filepath"].Replace("%2F", "/").Replace('/', '\\');
                }
            }
            catch //(Exception ex)
            {
                //log.WarnException("Failed to read URI query", ex);
            }
        }

        // This is not a complete query string parsing algorithm, but it's good enough for our needs.
        static IDictionary<string, string> ParseQueryString(Uri uri)
        {
            Debug.Assert(uri.Query.StartsWith('?'),
                String.Format(CultureInfo.InvariantCulture, "Uri.Query doesn't start with '?': '{0}'", uri.Query));
            return uri.Query.Substring(1).Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('='))
                .ToDictionary(pair => pair.First(), pair => pair.Length > 1 ? pair[1] : null,
                    StringComparer.OrdinalIgnoreCase);
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
                return true;
            }
            return false;
        }

        public string Branch { get; private set; }
        public string PullRequest { get; set; }
        public string Host { get; private set; }

        public string Owner { get; private set; }

        public string RepositoryName { get; private set; }

        public string NameWithOwner { get; private set; }

        public string RelativePathToOpen { get; private set; }

        public bool IsFileUri { get; private set; }

        public bool IsValidUri
        {
            get { return url != null; }
        }

        public Uri ToUri()
        {
            if (url == null)
                throw new InvalidOperationException("This Uri String is not a valid Uri");
            return url;
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
            if (uriString == null) return null;

            return uriString.Value;
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
                return urlBuilder.Uri.ToUriString();
            }
            return String.Concat(Value, addition);
        }

        public override string ToString()
        {
            // Makes this look better in the debugger.
            return Value;
        }

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
            return path == null ? null : path.Replace('\\', '/');
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
            return other != null && ToString().Equals(other.ToString());
        }
    }
}

namespace GitHub.Extensions
{
    using Primitives;
    public static class UriStringExtensions
    {
        public static UriString ToUriString(this Uri uri)
        {
            return uri == null ? null : new UriString(uri.ToString());
        }
    }
}