using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.ViewModels;
using System.Windows.Controls;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using GitHub.VisualStudio;

namespace GitHub.Exports
{
    /// <summary>
    /// Defines the type of a view or view model.
    /// </summary>
    public enum UIViewType
    {
        None,
        End,
        Login,
        TwoFactor,
        Create,
        Clone,
        Publish,
        Gist,
        PRList,
        PRDetail,
        PRCreation,
        LogoutRequired,
        GitHubPane,
        LoggedOut,
        NotAGitRepository,
        NotAGitHubRepository,
        StartPageClone,

    }

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
    /// A MEF export attribute that defines an export of type <see cref="IViewModel"/> with
    /// <see cref="UIViewType"/> metadata.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportViewModelAttribute : ExportAttribute
    {
        public ExportViewModelAttribute() : base(typeof(IViewModel))
        {}

        public UIViewType ViewType { get; set; }
    }

    /// <summary>
    /// A MEF export attribute that defines an export of type <see cref="IView"/> with
    /// <see cref="UIViewType"/> metadata.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportViewAttribute : ExportAttribute
    {
        public ExportViewAttribute() : base(typeof(IView))
        {
        }

        public UIViewType ViewType { get; set; }
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
    /// Defines a MEF metadata view that matches <see cref="ExportViewModelAttribute"/> and
    /// <see cref="ExportViewAttribute"/>.
    /// </summary>
    /// <remarks>
    /// For more information see the Metadata and Metadata views section at
    /// https://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx#Anchor_3
    /// </remarks>
    public interface IViewModelMetadata
    {
        UIViewType ViewType { get; }
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
        public static bool IsViewType(this UserControl c, UIViewType type)
        {
            return c.GetType().GetCustomAttributesData().Any(attr => IsViewType(attr, type));
        }

        public static bool IsViewType(this IView c, UIViewType type)
        {
            return c.GetType().GetCustomAttributesData().Any(attr => IsViewType(attr, type));
        }

        static bool IsViewType(CustomAttributeData attributeData, UIViewType viewType)
        {
            Debug.Assert(attributeData.NamedArguments != null);
            return attributeData.AttributeType == typeof(ExportViewAttribute)
                && (UIViewType)attributeData.NamedArguments[0].TypedValue.Value == viewType;
        }

        public static bool IsMenuType(this IMenuHandler c, MenuType type)
        {
            return c.GetType().GetCustomAttributesData().Any(attr => IsMenuType(attr, type));
        }

        static bool IsMenuType(CustomAttributeData attributeData, MenuType type)
        {
            Debug.Assert(attributeData.NamedArguments != null);
            return attributeData.AttributeType == typeof(ExportMenuAttribute)
                && (MenuType)attributeData.NamedArguments[0].TypedValue.Value == type;
        }
    }
}
