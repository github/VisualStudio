using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel.Composition;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Services
{

    [Export(typeof(IPullRequestStatusManager))]
    public class PullRequestStatusManager : IPullRequestStatusManager
    {
        const string SccStatusBarName = "SccStatusBar";
        const string GitHubStatusName = "GitHubStatusView";

        readonly Window mainWindow;

        public PullRequestStatusManager()
        {
            mainWindow = Application.Current.MainWindow;
        }

        public void ShowStatus()
        {
            var statusBar = FindSccStatusBar();
            if (statusBar != null)
            {
                var githubStatusBar = FindPullRequestStatusView(statusBar);
                if (githubStatusBar == null)
                {
                    var viewModel = new PullRequestStatusViewModel { Number = 666, Title = "A beast of a PR" };
                    var view = new PullRequestStatusView { Name = GitHubStatusName, DataContext = viewModel };
                    statusBar.Items.Insert(0, view);
                }
            }
        }

        public void HideStatus()
        {
            var statusBar = FindSccStatusBar();
            if (statusBar != null)
            {
                var githubStatusBar = FindPullRequestStatusView(statusBar);
                if (githubStatusBar != null)
                {
                    statusBar.Items.Remove(githubStatusBar);
                }
            }
        }

        static UserControl FindPullRequestStatusView(StatusBar statusBar)
        {
            return FindChild<UserControl>(statusBar, GitHubStatusName);
        }

        StatusBar FindSccStatusBar()
        {
            return FindChild<StatusBar>(mainWindow, SccStatusBarName);
        }

        // https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
        static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        class PullRequestStatusManagerInstaller
        {
            [STAThread]
            void Install()
            {
                var provider = new PullRequestStatusManager();
                provider.HideStatus();
                provider.ShowStatus();
            }
        }
    }
}
