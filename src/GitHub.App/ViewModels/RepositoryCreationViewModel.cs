using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using NLog;
using NullGuard;
using Octokit;
using ReactiveUI;
using Rothko;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : RepositoryFormViewModel, IRepositoryCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        readonly ObservableAsPropertyHelper<IReadOnlyList<LicenseItem>> licenses;
        readonly ObservableAsPropertyHelper<IReadOnlyList<GitIgnoreItem>> gitIgnoreTemplates;
        readonly IRepositoryHost repositoryHost;
        readonly IRepositoryCreationService repositoryCreationService;
        readonly ObservableAsPropertyHelper<bool> isCreating;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly IOperatingSystem operatingSystem;

        [ImportingConstructor]
        RepositoryCreationViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            IOperatingSystem operatingSystem,
            IRepositoryCreationService repositoryCreationService,
            IAvatarProvider avatarProvider)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, operatingSystem, repositoryCreationService, avatarProvider)
        {}

        public RepositoryCreationViewModel(
            IRepositoryHost repositoryHost,
            IOperatingSystem operatingSystem,
            IRepositoryCreationService repositoryCreationService,
            IAvatarProvider avatarProvider)
        {
            this.repositoryHost = repositoryHost;
            this.operatingSystem = operatingSystem;
            this.repositoryCreationService = repositoryCreationService;

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CreateTitle, repositoryHost.Title);
            SelectedGitIgnoreTemplate = GitIgnoreItem.None;
            SelectedLicense = LicenseItem.None;

            accounts = repositoryHost.ModelService.GetAccounts()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Accounts, initialValue: new ReadOnlyCollection<IAccount>(new IAccount[] {}));
            
            this.WhenAny(x => x.Accounts, x => x.Value)
                .WhereNotNull()
                .Where(accts => accts.Any())
                .Subscribe(accts => {
                    var selectedAccount = accts.FirstOrDefault();
                    if (selectedAccount != null)
                    {
                        SelectedAccount = accts.FirstOrDefault();
                    }
                });

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());

            BaseRepositoryPathValidator = this.CreateBaseRepositoryPathValidator();

            var nonNullRepositoryName = this.WhenAny(
                x => x.RepositoryName,
                x => x.BaseRepositoryPath,
                (x, y) => x.Value)
                .WhereNotNull();

            RepositoryNameValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .IfNullOrEmpty(Resources.RepositoryNameValidatorEmpty)
                .IfTrue(x => x.Length > 100, Resources.RepositoryNameValidatorTooLong)
                .IfTrue(IsAlreadyRepoAtPath, Resources.RepositoryNameValidatorAlreadyExists);

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? String.Format(CultureInfo.CurrentCulture, Resources.SafeRepositoryNameWarning, parsedReference) : null;
                });

            this.WhenAny(x => x.BaseRepositoryPathValidator.ValidationResult, x => x.Value)
                .Subscribe();

            CreateRepository = InitializeCreateRepositoryCommand();

            canKeepPrivate = CanKeepPrivateObservable.CombineLatest(CreateRepository.IsExecuting,
                (canKeep, publishing) => canKeep && !publishing)
                .ToProperty(this, x => x.CanKeepPrivate);

            isCreating = CreateRepository.IsExecuting
                .ToProperty(this, x => x.IsCreating);

            gitIgnoreTemplates = repositoryHost.ModelService.GetGitIgnoreTemplates()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.GitIgnoreTemplates, initialValue: new GitIgnoreItem[] { });

            this.WhenAny(x => x.GitIgnoreTemplates, x => x.Value)
                .WhereNotNull()
                .Where(ignores => ignores.Any())
                .Subscribe(ignores =>
                {
                    SelectedGitIgnoreTemplate = ignores.FirstOrDefault(
                        template => template.Name.Equals("VisualStudio", StringComparison.OrdinalIgnoreCase));
                });

            licenses = repositoryHost.ModelService.GetLicenses()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Licenses, initialValue: new LicenseItem[] { });

            BaseRepositoryPath = repositoryCreationService.DefaultClonePath;
        }

        string baseRepositoryPath;
        /// <summary>
        /// Path to clone repositories into
        /// </summary>
        public string BaseRepositoryPath
        {
            [return: AllowNull]
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, StripSurroundingQuotes(value)); }
        }

        /// <summary>
        /// Fires up a file dialog to select the directory to clone into
        /// </summary>
        public ICommand BrowseForDirectory { get { return browseForDirectoryCommand; } }

        /// <summary>
        /// Is running the creation process
        /// </summary>
        public bool IsCreating { get { return isCreating.Value; } }

        /// <summary>
        /// If the repo can be made private (depends on the user plan)
        /// </summary>
        public bool CanKeepPrivate { get { return canKeepPrivate.Value; } }

        public IReadOnlyList<GitIgnoreItem> GitIgnoreTemplates
        {
            get { return gitIgnoreTemplates.Value; }
        }

        public IReadOnlyList<LicenseItem> Licenses
        {
            get { return licenses.Value; }
        }

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

        /// <summary>
        /// List of accounts (at least one)
        /// </summary>
        public IReadOnlyList<IAccount> Accounts { get { return accounts.Value; } }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator { get; private set; }

        /// <summary>
        /// Fires off the process of creating the repository remotely and then cloning it locally
        /// </summary>
        public IReactiveCommand<Unit> CreateRepository { get; private set; }

        protected override NewRepository GatherRepositoryInfo()
        {
            var gitHubRepository = base.GatherRepositoryInfo();

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

        IObservable<Unit> ShowBrowseForDirectoryDialog()
        {
            return Observable.Start(() =>
            {
                // We store this in a local variable to prevent it changing underneath us while the
                // folder dialog is open.
                var localBaseRepositoryPath = BaseRepositoryPath;
                var browseResult = operatingSystem.Dialog.BrowseForDirectory(localBaseRepositoryPath,
                    Resources.BrowseForDirectory);

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
            bool isAlreadyRepoAtPath = false;
            var validationResult = BaseRepositoryPathValidator.ValidationResult;
            string safeRepositoryName = GetSafeRepositoryName(potentialRepositoryName);
            if (validationResult != null && validationResult.IsValid)
            {
                if (Path.GetInvalidPathChars().Any(chr => BaseRepositoryPath.Contains(chr)))
                    return false;
                string potentialPath = Path.Combine(BaseRepositoryPath, safeRepositoryName);
                isAlreadyRepoAtPath = IsGitRepo(potentialPath);
            }
            return isAlreadyRepoAtPath;
        }

        bool IsGitRepo(string path)
        {
            try
            {
                return operatingSystem.File.Exists(Path.Combine(path, ".git", "HEAD"));
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }

        IObservable<Unit> OnCreateRepository(object state)
        {
            var newRepository = GatherRepositoryInfo();

            return repositoryCreationService.CreateRepository(
                newRepository,
                SelectedAccount,
                BaseRepositoryPath,
                repositoryHost.ApiClient);
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
                if (!Extensions.ExceptionExtensions.IsCriticalException(ex))
                {
                    log.Error("Error creating repository.", ex);
                    UserError.Throw(TranslateRepositoryCreateException(ex));
                }
            });

            return createCommand;
        }

        static string StripSurroundingQuotes(string path)
        {
            if (string.IsNullOrEmpty(path)
                || path.Length < 2
                || !path.StartsWith("\"", StringComparison.Ordinal)
                || !path.EndsWith("\"", StringComparison.Ordinal))
            {
                return path;
            }

            return path.Substring(1, path.Length - 2);
        }

        PublishRepositoryUserError TranslateRepositoryCreateException(Exception ex)
        {
            var existsException = ex as RepositoryExistsException;
            if (existsException != null && SelectedAccount != null)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.RepositoryCreationFailedAlreadyExists,
                    SelectedAccount.Login, RepositoryName);
                return new PublishRepositoryUserError(message, Resources.RepositoryCreationFailedAlreadyExistsMessage);
            }
            var quotaExceededException = ex as PrivateRepositoryQuotaExceededException;
            if (quotaExceededException != null && SelectedAccount != null)
            {
                return new PublishRepositoryUserError(Resources.RepositoryCreationFailedQuota, quotaExceededException.Message);
            }
            return new PublishRepositoryUserError(ex.Message);
        }
    }
}
