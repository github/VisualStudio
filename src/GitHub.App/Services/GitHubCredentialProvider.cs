using System;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Primitives;
using LibGit2Sharp;
using Microsoft.VisualStudio.Shell;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IGitHubCredentialProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GitHubCredentialProvider : IGitHubCredentialProvider
    {
        static readonly ILogger log = LogManager.ForContext<GitHubCredentialProvider>();
        readonly IKeychain keychain;

        [ImportingConstructor]
        public GitHubCredentialProvider(IKeychain keychain)
        {
            Guard.ArgumentNotNull(keychain, nameof(keychain));

            this.keychain = keychain;
        }

        /// <summary>
        /// This is a callback from libgit2
        /// </summary>
        /// <returns></returns>
        public Credentials HandleCredentials(string url, string username, SupportedCredentialTypes types)
        {
            if (url == null)
                return null; // wondering if we should return DefaultCredentials instead

            var host = HostAddress.Create(url);

            try
            {
                var credentials = ThreadHelper.JoinableTaskFactory.Run(async () => await keychain.Load(host));
                return new UsernamePasswordCredentials
                {
                    Username = credentials.Item1,
                    Password = credentials.Item2,
                };
            }
            catch (Exception e)
            {
                log.Error(e, "Error loading credentials in GitHubCredentialProvider");
                return null;
            }
        }
    }
}