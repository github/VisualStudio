using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for CreateIssueDialog.xaml
    /// </summary>
    public partial class CreateIssueDialog : DialogWindow
    {
        public CreateIssueDialog()
        {
            InitializeComponent();
        }

        public void ShowModal(string subject)
        {
            subjectTextBox.Text = subject;
            ShowModal();
        }
    }
}
