using System.Windows;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using GitHub.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Clone)]
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
