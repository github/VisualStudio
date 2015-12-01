using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericGistCreationControl : SimpleViewUserControl<IGistCreationViewModel, GistCreationControl>
    { }

    /// <summary>
    /// Interaction logic for GistCreationControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)] 
    public partial class GistCreationControl
    {
        public GistCreationControl()
        {
            InitializeComponent();
        }
    }
}
