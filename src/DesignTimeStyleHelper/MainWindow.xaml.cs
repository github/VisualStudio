using System;
using System.Windows;
using GitHub.SampleData;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio;

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
            gitHubHomeSection.DataContext = new GitHubHomeSectionDesigner();
        }

        private void cloneLink_Click(object sender, RoutedEventArgs e)  
        {
            var ui = App.ServiceProvider.GetExportedValue<IUIProvider>();

            var factory = ui.GetService<ExportFactoryProvider>();
            var d = factory.UIControllerFactory.CreateExport();
            var creation = d.Value.SelectFlow(UIControllerFlow.Clone);
            var x = new WindowController(creation);
            creation.Subscribe(_ => { }, _ => x.Close());
            x.Show();
            d.Value.Start();
        }

        private void createLink_Click(object sender, RoutedEventArgs e)
        {
            var ui = App.ServiceProvider.GetExportedValue<IUIProvider>();

            var factory = ui.GetService<ExportFactoryProvider>();
            var d = factory.UIControllerFactory.CreateExport();
            var creation = d.Value.SelectFlow(UIControllerFlow.Create);
            var x = new WindowController(creation);
            creation.Subscribe(_ => { }, _ => x.Close());
            x.Show();
            d.Value.Start();
        }
    }
}
