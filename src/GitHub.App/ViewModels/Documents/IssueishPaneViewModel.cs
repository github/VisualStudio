using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.Documents
{
    [Export(typeof(IIssueishPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueishPaneViewModel : ViewModelBase, IIssueishPaneViewModel
    {
        public Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            return Task.CompletedTask;
        }

        public Task Load(IConnection connection, string owner, string name, int number)
        {
            return Task.CompletedTask;
        }
    }
}
