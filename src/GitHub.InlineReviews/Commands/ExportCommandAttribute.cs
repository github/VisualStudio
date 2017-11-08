using System;
using System.ComponentModel.Composition;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Exports a <see cref="VsCommand"/>.
    /// </summary>
    /// <remarks>
    /// To implement a new command, inherit from the <see cref="VsCommand"/> or <see cref="Command{TParam}"/>
    /// class and add an <see cref="ExportCommandAttribute"/> to the class with the type of the package that
    /// the command is registered by.
    /// </remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    sealed class ExportCommandAttribute : ExportAttribute
    {
        public ExportCommandAttribute(Type packageType)
            : base(typeof(IPackageResource))
        {
            PackageType = packageType;
        }

        public Type PackageType { get; }
    }
}
