using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorer.Sync;

namespace GitHub.VisualStudio.UI.Views
{
    public partial class SynchronizeForkWithUpstream : UserControl
    {
        public SynchronizeForkWithUpstream()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as ISynchronizeForkWithUpstreamSection;
        }

        void synchronize_Click(object sender, RoutedEventArgs e)
        {
            synchronize.IsEnabled = false;
            ViewModel.Synchronize();
            synchronize.IsEnabled = true;
        }

        public ISynchronizeForkWithUpstreamSection ViewModel
        {
            get { return (ISynchronizeForkWithUpstreamSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(ISynchronizeForkWithUpstreamSection),
                typeof(SynchronizeForkWithUpstream));
    }
}
