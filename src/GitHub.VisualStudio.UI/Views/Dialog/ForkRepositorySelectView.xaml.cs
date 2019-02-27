using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.Models;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IForkRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ForkRepositorySelectView : UserControl
    {
        public ForkRepositorySelectView()
        {
            InitializeComponent();
        }

        private void accountsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var account = e.AddedItems.OfType<IAccount>().FirstOrDefault();

            if (account != null)
            {
                ((IForkRepositorySelectViewModel)DataContext).SelectedAccount.Execute(account).Subscribe();
            }
        }

        private void existingForksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var repository = e.AddedItems.OfType<RemoteRepositoryModel>().FirstOrDefault();
            if (repository != null)
            {
                ((IForkRepositorySelectViewModel)DataContext).SwitchOrigin.Execute(repository).Subscribe();
            }
        }
    }
}
