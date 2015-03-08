using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    public class CloneRepositoryViewModel : ICloneRepositoryViewModel
    {
        public string Title { get { return "Clone a GitHub Repository"; } } // TODO: this needs to be contextual

        IReactiveCommand<object> cloneCommand = ReactiveCommand.Create();

        public ICommand CloneCommand { get { return cloneCommand; } }

        public ICollection<IRepositoryModel> Repositories
        {
            get;
            private set;
        }

        [ImportingConstructor]
        public CloneRepositoryViewModel(IRepositoryHosts hosts)
        {
            // TODO: How do I know which host this dialog is associated with?

            Repositories = new List<IRepositoryModel>
            {
                new RepositoryModel {Owner = "haacked", Name = "encourage" },
                new RepositoryModel {Owner = "haacked", Name = "seegit" },
                new RepositoryModel {Owner = "haacked", Name = "routemagic" },
                new RepositoryModel {Owner = "haacked", Name = "subtext" },
                new RepositoryModel {Owner = "haacked", Name = "haacked.com" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.net" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.rb" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.objc" },
                new RepositoryModel {Owner = "github", Name = "github" },
                new RepositoryModel {Owner = "github", Name = "windows" },
                new RepositoryModel {Owner = "github", Name = "mac" },
                new RepositoryModel {Owner = "github", Name = "desktop" },
            };
        }
    }
}
