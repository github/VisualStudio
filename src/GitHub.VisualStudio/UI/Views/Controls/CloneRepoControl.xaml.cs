using GitHub.UI;
using GitHub.UI.Helpers;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;
using System.Windows;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [Export(typeof(IViewFor<ICloneRepoViewModel>))]
    public partial class CloneRepoControl : IViewFor<ICloneRepoViewModel>
    {
        public CloneRepoControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);
            InitializeComponent();
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
           "ViewModel", typeof(ICloneRepoViewModel), typeof(LoginControl), new PropertyMetadata(null));


        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ICloneRepoViewModel)value; }
        }

        public ICloneRepoViewModel ViewModel
        {
            [return: AllowNull]
            get
            { return (ICloneRepoViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
