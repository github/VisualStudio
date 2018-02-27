using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.VisualStudio.Views
{
    public partial class ExpandoLabelView : UserControl
    {
        public static readonly DependencyProperty IsExpandedProperty =
            Expander.IsExpandedProperty.AddOwner(typeof(ExpandoLabelView));

        public ExpandoLabelView()
        {
            InitializeComponent();
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            SetCurrentValue(IsExpandedProperty, true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            SetCurrentValue(IsExpandedProperty, false);
        }
    }
}
