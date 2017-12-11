using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using GitHub.VisualStudio;

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
    /// A MEF export attribute that defines an export of type <see cref="IMenuHandler"/> with
    /// <see cref="MenuType"/> metadata.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportMenuAttribute : ExportAttribute
    {
        public ExportMenuAttribute() : base(typeof(IMenuHandler))
        {
        }

        public MenuType MenuType { get; set; }
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

    /// <summary>
    /// Defines a MEF metadata view that matches <see cref="ExportMenuAttribute"/>.
    /// </summary>
    /// <remarks>
    /// For more information see the Metadata and Metadata views section at
    /// https://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx#Anchor_3
    /// </remarks>
    public interface IMenuMetadata
    {
        MenuType MenuType { get; }
    }

    public static class ExportMetadataAttributeExtensions
    {
        public static bool IsMenuType(this IMenuHandler c, MenuType type)
        {
            return c.GetType().GetCustomAttributesData().Any(attr => IsMenuType(attr, type));
        }

        static bool IsMenuType(CustomAttributeData attributeData, MenuType type)
        {
            if (attributeData.NamedArguments == null)
            {
                throw new GitHubLogicException("attributeData.NamedArguments may not be null");
            }

            return attributeData.AttributeType == typeof(ExportMenuAttribute)
                && (MenuType)attributeData.NamedArguments[0].TypedValue.Value == type;
        }
    }
}
