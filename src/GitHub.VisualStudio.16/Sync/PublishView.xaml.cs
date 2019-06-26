using System.Windows;
using System.Windows.Controls;

namespace GitHub.VisualStudio.Sync
{
    public partial class PublishView : UserControl
    {
        public PublishView()
        {
            InitializeComponent();
        }

        public PublishSection ParentSection
        {
            get { return (PublishSection)GetValue(ParentSectionProperty); }
            set { SetValue(ParentSectionProperty, value); }
        }
        public static readonly DependencyProperty ParentSectionProperty =
            DependencyProperty.Register("ParentSection", typeof(PublishSection), typeof(PublishView));
    }
}
