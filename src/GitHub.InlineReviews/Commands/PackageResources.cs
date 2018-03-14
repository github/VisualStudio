using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
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
        public static void Register<TPackage>(
            TPackage package,
            ExportProvider exportProvider,
            IMenuCommandService menuService)
                where TPackage : Package
        {
            var commands = exportProvider.GetExports<IPackageResource, IExportCommandMetadata>();

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    if (command.Metadata.PackageType == typeof(TPackage))
                    {
                        command.Value.Register(package, menuService);
                    }
                }
            }
        }
    }
}
