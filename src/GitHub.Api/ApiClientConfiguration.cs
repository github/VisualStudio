using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GitHub.Primitives;

namespace GitHub.Api
{
    /// <summary>
    /// Holds the configuration for API clients.
    /// </summary>
    public static class ApiClientConfiguration
    {
        /// <summary>
        /// Initializes static members of the <see cref="ApiClientConfiguration"/> class.
        /// </summary>
        static ApiClientConfiguration()
        {
            // HACK: The client ID and secret are set up as properties on ApiClient
            // for the moment, so we have to create an instance of that class to get
            // hold of them. TODO: Move them here.
            var gitHubApp = Assembly.Load("GitHub.App");
            var apiClientType = gitHubApp.GetType("GitHub.Api.ApiClient");
            var octokitReactive = Assembly.Load("Octokit.Reactive");
            var observableGitHubClientType = octokitReactive.GetType("Octokit.Reactive.IObservableGitHubClient");
            var ctor = apiClientType.GetConstructor(new[] { typeof(HostAddress), observableGitHubClientType });
            dynamic apiClient = ctor.Invoke(new object[] { null, null });

            ClientId = apiClient.ClientId;
            ClientSecret = apiClient.ClientSecret;
        }

        /// <summary>
        /// Gets the application's OAUTH client ID.
        /// </summary>
        public static string ClientId { get; }

        /// <summary>
        /// Gets the application's OAUTH client secret.
        /// </summary>
        public static string ClientSecret { get; }

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
                return GetSha256Hash(Info.ApplicationInfo.ApplicationDescription + ":" + GetMachineIdentifier());
            }
        }

        static string GetMachineIdentifier()
        {
            try
            {
                // adapted from http://stackoverflow.com/a/1561067
                var fastedValidNetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .OrderBy(nic => nic.Speed)
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault(address => address.Length > 12);

                return fastedValidNetworkInterface ?? GetMachineNameSafe();
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
