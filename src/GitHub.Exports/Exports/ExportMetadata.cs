using System;
using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.ViewModels;
using System.Windows.Controls;
using System.Linq;

namespace GitHub.Exports {

	public enum UIViewType {
        None,
		Login,
		TwoFactor,
		Create,
		Clone,
        Publish,
        End = 100,
        Finished
	}

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExportViewModelAttribute : ExportAttribute
    {
        public ExportViewModelAttribute() : base(typeof(IViewModel))
        {
        }

        public UIViewType ViewType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ExportViewAttribute : ExportAttribute
    {
        public ExportViewAttribute() : base(typeof(IView))
        {
        }

        public UIViewType ViewType { get; set; }
    }

    public interface IViewModelMetadata
    {
        UIViewType ViewType { get; }
    }

    public static class ExportViewAttributeExtensions
    {
        public static bool IsViewType(this UserControl c, UIViewType type)
        {
            return c.GetType().GetCustomAttributesData().Any(x => x.AttributeType.Equals(typeof(ExportViewAttribute)) && (UIViewType)x.NamedArguments[0].TypedValue.Value == type);
        }
    }
}