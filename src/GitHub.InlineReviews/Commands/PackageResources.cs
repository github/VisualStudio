using System;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews.Commands
{
    static class PackageResources
    {
        /// <summary>
        /// Registers the resources for a package.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package.</typeparam>
        /// <param name="package">The package.</param>
        public static void Register<TPackage>(TPackage package) where TPackage : Package
        {
            var serviceProvider = package.GetServiceSafe<IGitHubServiceProvider>();
            var commands = serviceProvider?.ExportProvider?.GetExports<IPackageResource, IExportCommandMetadata>();

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    if (command.Metadata.PackageType == typeof(TPackage))
                    {
                        command.Value.Register(package);
                    }
                }
            }
        }
    }
}
