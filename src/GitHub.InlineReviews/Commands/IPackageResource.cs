using System;
using System.ComponentModel.Design;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Represents a resource to be registered on package initialization.
    /// </summary>
    public interface IPackageResource
    {
        /// <summary>
        /// Registers the resource with a package.
        /// </summary>
        /// <param name="package">The package registering the resource.</param>
        /// <param name="menuService">The menu service to register against.</param>
        /// <remarks>
        /// This method should not be called directly, instead packages should call
        /// <see cref="PackageResources.Register{TPackage}(TPackage)"/> on initialization.
        /// </remarks>
        void Register(IServiceProvider package, IMenuCommandService menuService);
    }
}
