using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Validation;
using NLog;
using NullGuard;
using Octokit.Reactive;
using ReactiveUI;
using Rothko;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : ReactiveObject, IRepositoryCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ObservableAsPropertyHelper<string> safeRepositoryName;
        readonly IOperatingSystem operatingSystem;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ReactiveCommand<object> createRepositoryCommand = ReactiveCommand.Create();

        [ImportingConstructor]
        public RepositoryCreationViewModel(IOperatingSystem operatingSystem, IRepositoryHosts hosts)
        {
            this.operatingSystem = operatingSystem;

            Accounts = new ReactiveList<IAccount>();

            safeRepositoryName = this.WhenAny(x => x.RepositoryName, x => x.Value)
                .Select(x => x != null ? GetSafeRepositoryName(x) : null)
                .ToProperty(this, x => x.SafeRepositoryName);

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());

            var nonNullRepositoryName = this.WhenAny(
                x => x.RepositoryName,
                x => x.BaseRepositoryPath,
                (x, y) => x.Value)
                .WhereNotNull();

            RepositoryNameValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .IfNullOrEmpty("Please enter a repository name")
                .IfTrue(x => x.Length > 100, "Repository name must be fewer than 100 characters")
                .IfTrue(x => IsAlreadyRepoAtPath(GetSafeRepositoryName(x)), "Repository with same name already exists at this location");

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(this.WhenAny(x => x.BaseRepositoryPath, x => x.Value))
                .IfNullOrEmpty("Please enter a repository path")
                .IfTrue(x => x.Length > 200, "Path too long")
                .IfContainsInvalidPathChars("Path contains invalid characters")
                .IfPathNotRooted("Please enter a valid path");

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? "Will be created as " + parsedReference : null;
                });

            GitIgnoreTemplates = new ReactiveList<GitIgnoreItem>();

            hosts.GitHubHost.ApiClient
                .GetGitIgnoreTemplates()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(templateName => new GitIgnoreItem(templateName))
                .ToList()
                .Subscribe(templates => GitIgnoreTemplates.AddRange(templates));
        }

        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public ReactiveList<IAccount> Accounts
        {
            get;
            private set;
        }

        public ReactiveList<GitIgnoreItem> GitIgnoreTemplates
        {
            get;
            private set;
        }

        string baseRepositoryPath;
        public string BaseRepositoryPath
        {
            [return: AllowNull]
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, value); }
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }

        public ICommand BrowseForDirectory
        {
            get { return browseForDirectoryCommand; }
        }

        public bool CanKeepPrivate
        {
            get;
            private set;
        }

        public ICommand CreateRepository
        {
            get { return createRepositoryCommand; }
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
            get { return repositoryName; }
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
            get { return safeRepositoryName.Value; }
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

        IObservable<Unit> ShowBrowseForDirectoryDialog()
        {
            return Observable.Start(() =>
            {
                // We store this in a local variable to prevent it changing underneath us while the
                // folder dialog is open.
                var localBaseRepositoryPath = BaseRepositoryPath;
                var browseResult = operatingSystem.Dialog.BrowseForDirectory(localBaseRepositoryPath,
                    "Select a containing folder for your new repository.");

                if (!browseResult.Success)
                    return;

                var directory = browseResult.DirectoryPath ?? localBaseRepositoryPath;

                try
                {
                    BaseRepositoryPath = directory;
                }
                catch (Exception e)
                {
                    // TODO: We really should limit this to exceptions we know how to handle.
                    log.Error(string.Format(CultureInfo.InvariantCulture,
                        "Failed to set base repository path.{0}localBaseRepositoryPath = \"{1}\"{0}BaseRepositoryPath = \"{2}\"{0}Chosen directory = \"{3}\"",
                        System.Environment.NewLine, localBaseRepositoryPath ?? "(null)", BaseRepositoryPath ?? "(null)", directory ?? "(null)"), e);
                }
            }, RxApp.MainThreadScheduler);
        }

        bool IsAlreadyRepoAtPath(string potentialRepositoryName)
        {
            bool isValid = false;
            var validationResult = BaseRepositoryPathValidator.ValidationResult;
            if (validationResult != null && validationResult.IsValid)
            {
                string potentialPath = Path.Combine(BaseRepositoryPath, potentialRepositoryName);
                isValid = IsGitRepo(potentialPath);
            }
            return isValid;
        }

        bool IsGitRepo(string path)
        {
            try
            {
                var head = operatingSystem.File.GetFile(Path.Combine(path, ".git", "HEAD"));
                return head.Exists;
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }
    }
}
