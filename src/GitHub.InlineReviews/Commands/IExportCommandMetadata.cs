using System;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Defines a MEF metadata view that matches <see cref="ExportCommandAttribute"/>.
    /// </summary>
    /// <remarks>
    /// For more information see the Metadata and Metadata views section at
    /// https://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx#Anchor_3
    /// </remarks>
    public interface IExportCommandMetadata
    {
        Type PackageType { get; }
    }
}
