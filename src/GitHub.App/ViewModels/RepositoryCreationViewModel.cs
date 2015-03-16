using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;
using GitHub.Exports;
using System.ComponentModel.Composition;
using NullGuard;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : ReactiveObject, IRepositoryCreationViewModel
    {
        ObservableAsPropertyHelper<string> safeRepositoryName;
        readonly ObservableAsPropertyHelper<string> safeRepositoryName;

        public RepositoryCreationViewModel()
        {
            safeRepositoryName = this.WhenAny(x => x.RepositoryName, x => x.Value)
            .Select(x => x != null ? GetSafeRepositoryName(x) : null)
            .ToProperty(this, x => x.SafeRepositoryName);
        }

        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public ReactiveList<IAccount> Accounts
        {
            get;
            private set;
        }

        public string BaseRepositoryPath
        {
            get;
            set;
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }

        public ICommand BrowseForDirectory
        {
            get;
            private set;
        }

        public bool CanKeepPrivate
        {
            get;
            private set;
        }

        public ICommand CreateRepository
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool IsPublishing
        {
            get;
            private set;
        }

        public bool KeepPrivate
        {
            get;
            set;
        }

        string repositoryName;
        [AllowNull]
        public string RepositoryName
        {
            [return: AllowNull]
            get
            { return repositoryName; }
            set { this.RaiseAndSetIfChanged(ref repositoryName, value); }
        }

        public ReactivePropertyValidator<string> RepositoryNameValidator
        {
            get;
            private set;
        }

        public string SafeRepositoryName
        {
            [return: AllowNull]
            get
            {
                return safeRepositoryName.Value;
            }
        }

        public ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator
        {
            get;
            private set;
        }

        public string RepositoryNameWarningText
        {
            get;
            private set;
        }

        public ICommand Reset
        {
            get;
            private set;
        }

        public IAccount SelectedAccount
        {
            get;
            private set;
        }

        public bool ShowRepositoryNameWarning
        {
            get;
            private set;
        }

        public bool ShowUpgradePlanWarning
        {
            get;
            private set;
        }

        public bool ShowUpgradeToMicroPlanWarning
        {
            get;
            private set;
        }

        public ICommand UpgradeAccountPlan
        {
            get;
            private set;
        }

        // These are the characters which are permitted when creating a repository name on GitHub The Website
        static readonly Regex invalidRepositoryCharsRegex = new Regex(@"[^0-9A-Za-z_\.\-]", RegexOptions.ECMAScript);

        /// <summary>
        /// Given a repository name, returns a safe version with invalid characters replaced with dashes.
        /// </summary>
        static string GetSafeRepositoryName(string name)
        {
            return invalidRepositoryCharsRegex.Replace(name, "-");
        }
    }
}
