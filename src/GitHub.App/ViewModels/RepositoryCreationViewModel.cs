using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;
using Rothko;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : ReactiveObject, IRepositoryCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        // These are the characters which are permitted when creating a repository name on GitHub The Website
        static readonly Regex invalidRepositoryCharsRegex = new Regex(@"[^0-9A-Za-z_\.\-]", RegexOptions.ECMAScript);

        readonly ObservableAsPropertyHelper<string> safeRepositoryName;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly ObservableAsPropertyHelper<bool> isPublishing;
        readonly IOperatingSystem operatingSystem;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly IRepositoryCreationService repositoryCreationService;

        [ImportingConstructor]
        public RepositoryCreationViewModel(IOperatingSystem operatingSystem, IRepositoryHosts hosts,
            IRepositoryCreationService repositoryCreationService, IRepositoryCloneService cloneService)
        {
            this.operatingSystem = operatingSystem;
            RepositoryHost = hosts.GitHubHost;
            this.repositoryCreationService = repositoryCreationService;
            
            Accounts = RepositoryHost.Accounts ?? new ReactiveList<IAccount>();
            Debug.Assert(Splat.ModeDetector.InUnitTestRunner() || Accounts.Any(), "There must be at least one account");
            var selectedAccount = Accounts.FirstOrDefault();
            if (selectedAccount != null)
                SelectedAccount = Accounts.FirstOrDefault();

            SelectedGitIgnoreTemplate = GitIgnoreItem.None;
            SelectedLicense = LicenseItem.None;

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

            Observable.Return(GitIgnoreItem.None).Concat(
                RepositoryHost.ApiClient
                    .GetGitIgnoreTemplates()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(GitIgnoreItem.Create))
                .ToList()
                .Subscribe(templates =>
                {
                    GitIgnoreTemplates.AddRange(templates.OrderByDescending(template => template.Recommended));
                    Debug.Assert(GitIgnoreTemplates.Any(), "There should be at least one GitIgnoreTemplate");
                });

            Licenses = new ReactiveList<LicenseItem>();
            Observable.Return(LicenseItem.None).Concat(
                RepositoryHost.ApiClient
                    .GetLicenses()
                    .WhereNotNull()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(license => new LicenseItem(license)))
                .ToList()
                .Subscribe(licenses =>
                {
                    Licenses.AddRange(licenses.OrderByDescending(lic => lic.Recommended));
                    Debug.Assert(Licenses.Any(), "There should be at least one license");
                });

            CreateRepository = InitializeCreateRepositoryCommand();

            var canKeepPrivateObs = this.WhenAny(
                x => x.SelectedAccount.IsEnterprise,
                x => x.SelectedAccount.IsOnFreePlan,
                x => x.SelectedAccount.HasMaximumPrivateRepositories,
                (isEnterprise, isOnFreePlan, hasMaxPrivateRepos) =>
                    isEnterprise.Value || (!isOnFreePlan.Value && !hasMaxPrivateRepos.Value));

            canKeepPrivate = canKeepPrivateObs.CombineLatest(CreateRepository.IsExecuting,
                (canKeep, publishing) => canKeep && !publishing)
                .ToProperty(this, x => x.CanKeepPrivate);

            canKeepPrivateObs
                .Where(x => !x)
                .Subscribe(x => KeepPrivate = false);

            isPublishing = CreateRepository.IsExecuting
                .ToProperty(this, x => x.IsPublishing);

            BaseRepositoryPath = cloneService.GetLocalClonePathFromGitProvider(cloneService.DefaultClonePath);
        }

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

        Octokit.NewRepository GatherRepositoryInfo()
        {
            var gitHubRepository = new Octokit.NewRepository(RepositoryName)
            {
                Description = Description,
                Private = KeepPrivate
            };

            if (SelectedLicense != LicenseItem.None)
            {
                gitHubRepository.LicenseTemplate = SelectedLicense.Key;
                gitHubRepository.AutoInit = true;
            }

            if (SelectedGitIgnoreTemplate != GitIgnoreItem.None)
            {
                gitHubRepository.GitignoreTemplate = SelectedGitIgnoreTemplate.Name;
                gitHubRepository.AutoInit = true;
            }

            return gitHubRepository;
        }

        IObservable<Unit> OnCreateRepository(object state)
        {
            var newRepository = GatherRepositoryInfo();

            return repositoryCreationService.CreateRepository(
                newRepository,
                SelectedAccount,
                BaseRepositoryPath,
                RepositoryHost.ApiClient);
        }

        ReactiveCommand<Unit> InitializeCreateRepositoryCommand()
        {
            var canCreate = this.WhenAny(
                x => x.RepositoryNameValidator.ValidationResult.IsValid,
                x => x.BaseRepositoryPathValidator.ValidationResult.IsValid,
                (x, y) => x.Value && y.Value);
            var createCommand = ReactiveCommand.CreateAsyncObservable(canCreate, OnCreateRepository);
            createCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (!ex.IsCriticalException())
                {
                    // TODO: Throw a proper error.
                    log.Error("Error creating repository.", ex);
                    UserError.Throw(new PublishRepositoryUserError(ex.Message));
                }
            });

            return createCommand;
        }

        /// <summary>
        /// Title for the dialog
        /// </summary>
        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        /// <summary>
        /// Host owning the repos
        /// </summary>
        public IRepositoryHost RepositoryHost { get; private set; }

        /// <summary>
        /// List of accounts (at least one)
        /// </summary>
        public ReactiveList<IAccount> Accounts { get; private set; }

        /// <summary>
        /// List of .gitignore templates for the dropdown
        /// </summary>
        public ReactiveList<GitIgnoreItem> GitIgnoreTemplates { get; private set; }

        /// <summary>
        /// List of license templates for the dropdown
        /// </summary>
        public ReactiveList<LicenseItem> Licenses { get; private set; }

        string baseRepositoryPath;
        /// <summary>
        /// Path to clone repositories into
        /// </summary>
        public string BaseRepositoryPath
        {
            [return: AllowNull]
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, value); }
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; private set; }

        /// <summary>
        /// Fires up a file dialog to select the directory to clone into
        /// </summary>
        public ICommand BrowseForDirectory { get { return browseForDirectoryCommand; } }

        /// <summary>
        /// If the repo can be made private (depends on the user plan)
        /// </summary>
        public bool CanKeepPrivate { get { return canKeepPrivate.Value; } }

        /// <summary>
        /// Fires off the process of creating the repository remotely and then cloning it locally
        /// </summary>
        public IReactiveCommand<Unit> CreateRepository { get; private set; }

        string description;
        /// <summary>
        /// Description to set on the repo (optional)
        /// </summary>
        [AllowNull]
        public string Description
        {
            [return: AllowNull]
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        /// <summary>
        /// Is true when the CreateRepository command is in the process of executing
        /// </summary>
        public bool IsPublishing { get { return isPublishing.Value; } }

        bool keepPrivate;
        /// <summary>
        /// Make the new repository private
        /// </summary>
        public bool KeepPrivate
        {
            get { return keepPrivate; }
            set { this.RaiseAndSetIfChanged(ref keepPrivate, value); }
        }

        string repositoryName;
        /// <summary>
        /// Name of the repository as typed by user
        /// </summary>
        [AllowNull]
        public string RepositoryName
        {
            [return: AllowNull]
            get { return repositoryName; }
            set { this.RaiseAndSetIfChanged(ref repositoryName, value); }
        }

        public ReactivePropertyValidator<string> RepositoryNameValidator { get; private set; }

        /// <summary>
        /// Name of the repository after fixing it to be safe (dashes instead of spaces, etc)
        /// </summary>
        public string SafeRepositoryName
        {
            [return: AllowNull]
            get { return safeRepositoryName.Value; }
        }

        public ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator { get; private set; }

        /// <summary>
        /// Resets the form
        /// </summary>
        public ICommand Reset { get; private set; }

        IAccount selectedAccount;
        /// <summary>
        /// Account where the repository is going to be created on
        /// </summary>
        [AllowNull]
        public IAccount SelectedAccount
        {
            [return: AllowNull]
            get { return selectedAccount; }
            set { this.RaiseAndSetIfChanged(ref selectedAccount, value); }
        }

        public bool ShowUpgradePlanWarning { get; private set; }

        public bool ShowUpgradeToMicroPlanWarning { get; private set; }

        public ICommand UpgradeAccountPlan { get; private set; }

        GitIgnoreItem selectedGitIgnoreTemplate;
        [AllowNull]
        public GitIgnoreItem SelectedGitIgnoreTemplate
        {
            get { return selectedGitIgnoreTemplate; }
            set { this.RaiseAndSetIfChanged(ref selectedGitIgnoreTemplate, value ?? GitIgnoreItem.None); }
        }

        LicenseItem selectedLicense;
        [AllowNull]
        public LicenseItem SelectedLicense
        {
            get { return selectedLicense; }
            set { this.RaiseAndSetIfChanged(ref selectedLicense, value ?? LicenseItem.None); }
        }
    }
}
