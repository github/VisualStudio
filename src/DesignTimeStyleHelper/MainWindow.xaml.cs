using System.Windows;
using GitHub.VisualStudio.TeamExplorerConnect;

namespace DesignTimeStyleHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var section = (PlaceholderGitHubSection)App.ServiceProvider.GetService(typeof(PlaceholderGitHubSection));
            container.Children.Add(section.SectionContent as UIElement);

        }
    }
}
