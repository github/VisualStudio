using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace GitHub.Exports
{
    /// <summary>
    /// Defines the types of global visual studio menus available.
    /// </summary>
    public enum MenuType
    {
        GitHubPane,
        OpenPullRequests
    }

    /// <summary>
    /// Defines the type of repository link to navigate to
    /// </summary>
    public enum LinkType
    {
        Blob,
        Blame
    }

    /// <summary>
    /// A MEF export attribute that defines an export of type <see cref="FrameworkElement"/> with
    /// <see cref="ViewModelType"/> metadata.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
        Justification = "Store string rather than Type as metadata")]
    public sealed class ExportViewForAttribute : ExportAttribute
    {
        public ExportViewForAttribute(Type viewModelType)
            : base(typeof(FrameworkElement))
        {
            ViewModelType = viewModelType.FullName;
        }

        public string ViewModelType { get; }
    }

    /// <summary>
    /// Defines a MEF metadata view that matches <see cref="ExportViewModelForAttribute"/> and
    /// <see cref="ExportViewForAttribute"/>.
    /// </summary>
    /// <remarks>
    /// For more information see the Metadata and Metadata views section at
    /// https://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx#Anchor_3
    /// </remarks>
    public interface IViewModelMetadata
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] ViewModelType { get; }
    }
}
