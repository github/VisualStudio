using System.Windows.Controls;
using GitHub.Services;

namespace GitHub.VisualStudio.UI
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        public OptionsControl()
        {
            InitializeComponent();
        }

        public bool CollectMetrics
        {
            get { return chkMetrics.IsChecked ?? false; }
            set { chkMetrics.IsChecked = value; }
        }

        public bool EnableTraceLogging
        {
            get { return chkEnableTraceLogging.IsChecked ?? false; }
            set { chkEnableTraceLogging.IsChecked = value; }
        }

        public bool EditorComments
        {
            get { return chkEditorComments.IsChecked ?? false; }
            set { chkEditorComments.IsChecked = value; }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var browser = VisualStudio.Services.DefaultExportProvider.GetExportedValue<IVisualStudioBrowser>();
            browser?.OpenUrl(e.Uri);
        }
    }
}
