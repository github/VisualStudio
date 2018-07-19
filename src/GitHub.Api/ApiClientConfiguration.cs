using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace GitHub.Api
{
    /// <summary>
    /// Holds the configuration for API clients.
    /// </summary>
    public static partial class ApiClientConfiguration
    {
        /// <summary>
        /// Initializes static members of the <see cref="ApiClientConfiguration"/> class.
        /// </summary>
        static ApiClientConfiguration()
        {
            Configure();
        }

        /// <summary>
        /// Gets the application's OAUTH client ID.
        /// </summary>
        public static string ClientId { get; private set; }

        /// <summary>
        /// Gets the application's OAUTH client secret.
        /// </summary>
        public static string ClientSecret { get; private set; }

        /// <summary>
        /// Gets the scopes required by the application.
        /// </summary>
        public static IReadOnlyList<string> RequiredScopes { get; } = new[] { "user", "repo", "gist", "write:public_key", "read:org" };

        /// <summary>
        /// Gets a note that will be stored with the OAUTH token.
        /// </summary>
        public static string AuthorizationNote
        {
            get { return Info.ApplicationInfo.ApplicationDescription + " on " + GetMachineNameSafe(); }
        }

        /// <summary>
        /// Gets the machine fingerprint that will be registered with the OAUTH token, allowing
        /// multiple authorizations to be created for a single user.
        /// </summary>
        public static string MachineFingerprint
        {
            get
            {
                return GetSha256Hash(
                    Info.ApplicationInfo.ApplicationDescription + ":" +
                    GetMachineIdentifier() + ":" +
                    GetMachineNameSafe());
            }
        }

        static partial void Configure();

        static string GetMachineIdentifier()
        {
            try
            {
                // adapted from http://stackoverflow.com/a/1561067
                var fastestValidNetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .OrderByDescending(nic => nic.Speed)
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault(address => address.Length >= 12);

                return fastestValidNetworkInterface ?? GetMachineNameSafe();
            }
            catch (Exception)
            {
                //log.Info("Could not retrieve MAC address. Fallback to using machine name.", e);
                return GetMachineNameSafe();
            }
        }

        static string GetMachineNameSafe()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (Exception)
            {
                //log.Info("Failed to retrieve host name using `DNS.GetHostName`.", e);

                try
                {
                    return Environment.MachineName;
                }
                catch (Exception)
                {
                    //log.Info("Failed to retrieve host name using `Environment.MachineName`.", ex);
                    return "(unknown)";
                }
            }
        }

        static string GetSha256Hash(string input)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(input);
                    var hash = sha256.ComputeHash(bytes);

                    return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception)
            {
                //log.Error("IMPOSSIBLE! Generating Sha256 hash caused an exception.", e);
                return null;
            }
        }
    }
}
