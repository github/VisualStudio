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
    }

    public enum MenuType
    {
        GitHubPane,
        OpenPullRequests
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportViewModelAttribute : ExportAttribute
    {
        public ExportViewModelAttribute() : base(typeof(IViewModel))
        {}

        public UIViewType ViewType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportViewAttribute : ExportAttribute
    {
        public ExportViewAttribute() : base(typeof(IView))
        {
        }

        public UIViewType ViewType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportMenuAttribute : ExportAttribute
    {
        public ExportMenuAttribute() : base(typeof(IMenuHandler))
        {
        }

        public MenuType MenuType { get; set; }
    }

    public interface IViewModelMetadata
    {
        UIViewType ViewType { get; }
    }

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