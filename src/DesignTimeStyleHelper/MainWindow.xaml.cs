using System;
using System.Windows;
using GitHub.SampleData;
using GitHub.Services;
using GitHub.UI;
using GitHub.Extensions;
using GitHub.Models;

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

        private void loginLink_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(UIControllerFlow.Authentication);
        }

        private void cloneLink_Click(object sender, RoutedEventArgs e)  
        {
            ShowDialog(UIControllerFlow.Clone);
        }

        private void createLink_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(UIControllerFlow.Create);
        }

        private void publishLink_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(UIControllerFlow.Publish);
        }

        private void twoFactorTester_Click(object sender, RoutedEventArgs e)
        {
            var twoFactorTester = new TwoFactorInputTester();
            twoFactorTester.ShowDialog();
        }

        void ShowDialog(UIControllerFlow flow)
        {
            var ui = App.ServiceProvider.GetService<IUIProvider>();
            var controller = ui.Configure(flow);
            var userControlObservable = controller.TransitionSignal;
            var x = new WindowController(userControlObservable);
            userControlObservable.Subscribe(_ => { }, _ => x.Close());
            x.Show();
            ui.RunInDialog(controller);
        }
    }
}
