using System;

namespace GitHub.Exports {

	public enum UIViewType {
        None,
		Login,
		TwoFactor,
		Create,
		Clone,
        End = 100
	}


    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class ExportViewModelAttribute : ExportAttribute
    {
        public ExportViewModelAttribute() : base(typeof(IViewModel))
        {
        }

        public UIViewType ViewType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class ExportViewAttribute : ExportAttribute
    {
        public ExportViewAttribute() : base(typeof(IViewFor))
        {
        }

        public UIViewType ViewType { get; set; }
    }

    public interface IViewModelMetadata
    {
        public IUIViewType ViewType { get; }
    }
}