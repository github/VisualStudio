using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GitHub.Exports;
using GitHub.ViewModels.Dialog.Clone;

namespace GitHub.VisualStudio.Views.Dialog.Clone
{
    [ExportViewFor(typeof(IRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SelectPageView : UserControl
    {
        public SelectPageView()
        {
            InitializeComponent();

            // See Douglas Stockwell's suggestion:
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/30ed27ce-f7b7-48ae-8adc-0400b9b9ec78
            IsVisibleChanged += (sender, e) =>
            {
                if (IsVisible)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() => SearchTextBox.Focus()));
                }
            };
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }
    }
}
