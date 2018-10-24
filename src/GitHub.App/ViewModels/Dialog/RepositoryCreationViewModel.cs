using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.App;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using Octokit;
using ReactiveUI;
using Rothko;
using Serilog;
using IConnection = GitHub.Models.IConnection;
using UserError = ReactiveUI.Legacy.UserError;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IRepositoryCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : RepositoryFormViewModel, IRepositoryCreationViewModel
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryCreationViewModel>();

        readonly ReactiveCommand<Unit, Unit> browseForDirectoryCommand = ReactiveCommand.Create(() => { });
        readonly IModelServiceFactory modelServiceFactory;
        readonly IRepositoryCreationService repositoryCreationService;
        readonly ObservableAsPropertyHelper<bool> isCreating;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly IOperatingSystem operatingSystem;
        readonly IUsageTracker usageTracker;
        ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        IModelService modelService;

        [ImportingConstructor]
        public RepositoryCreationViewModel(
            IModelServiceFactory modelServiceFactory,
            IOperatingSystem operatingSystem,
            IRepositoryCreationService repositoryCreationService,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(operatingSystem, nameof(operatingSystem));
            Guard.ArgumentNotNull(repositoryCreationService, nameof(repositoryCreationService));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            this.modelServiceFactory = modelServiceFactory;
            this.operatingSystem = operatingSystem;
            this.repositoryCreationService = repositoryCreationService;
            this.usageTracker = usageTracker;

            SelectedGitIgnoreTemplate = GitIgnoreItem.None;
            SelectedLicense = LicenseItem.None;

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(this.WhenAny(x => x.BaseRepositoryPath, x => x.Value))
                .IfNullOrEmpty(Resources.RepositoryCreationClonePathEmpty)
                .IfTrue(x => x.Length > 200, Resources.RepositoryCreationClonePathTooLong)
                .IfContainsInvalidPathChars(Resources.RepositoryCreationClonePathInvalidCharacters)
                .IfPathNotRooted(Resources.RepositoryCreationClonePathInvalid);

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

            BaseRepositoryPath = repositoryCreationService.DefaultClonePath;
        }

        public string Title { get; private set; }

        string baseRepositoryPath;
        /// <summary>
        /// Path to clone repositories into
        /// </summary>
        public string BaseRepositoryPath
        {
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, StripSurroundingQuotes(value)); }
        }

        /// <summary>
        /// Fires up a file dialog to select the directory to clone into
        /// </summary>
        public ReactiveCommand<Unit, Unit> BrowseForDirectory { get { return browseForDirectoryCommand; } }

        /// <summary>
        /// Is running the creation process
        /// </summary>
        public bool IsCreating { get { return isCreating.Value; } }

        /// <summary>
        /// If the repo can be made private (depends on the user plan)
        /// </summary>
        public bool CanKeepPrivate { get { return canKeepPrivate.Value; } }

        IReadOnlyList<GitIgnoreItem> gitIgnoreTemplates;
        public IReadOnlyList<GitIgnoreItem> GitIgnoreTemplates
        {
            get { return gitIgnoreTemplates; }
            set { this.RaiseAndSetIfChanged(ref gitIgnoreTemplates, value); }
        }

        IReadOnlyList<LicenseItem> licenses;
        public IReadOnlyList<LicenseItem> Licenses
        {
            get { return licenses; }
            set { this.RaiseAndSetIfChanged(ref licenses, value); }
        }

        GitIgnoreItem selectedGitIgnoreTemplate;
        public GitIgnoreItem SelectedGitIgnoreTemplate
        {
            get { return selectedGitIgnoreTemplate; }
            set { this.RaiseAndSetIfChanged(ref selectedGitIgnoreTemplate, value ?? GitIgnoreItem.None); }
        }

        LicenseItem selectedLicense;
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
        public ReactiveCommand<Unit, Unit> CreateRepository { get; private set; }

        public IObservable<object> Done => CreateRepository.Select(_ => (object)null);

        public async Task InitializeAsync(IConnection connection)
        {
            modelService = await modelServiceFactory.CreateAsync(connection).ConfigureAwait(true);

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CreateTitle, connection.HostAddress.Title);

            accounts = modelService.GetAccounts()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Accounts, initialValue: new ReadOnlyCollection<IAccount>(Array.Empty<IAccount>()));

            this.WhenAny(x => x.Accounts, x => x.Value)
                .Select(accts => accts?.FirstOrDefault())
                .WhereNotNull()
                .Subscribe(a => SelectedAccount = a);

            modelService.GetGitIgnoreTemplates()
                .Where(x => x != null)
                .ToList()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    var sorted = x
                        .Distinct()
                        .OrderByDescending(item => item.Recommended)
                        .ThenBy(item => item.Name);
                    GitIgnoreTemplates = new[] { GitIgnoreItem.None }.Concat(sorted).ToList();

                    SelectedGitIgnoreTemplate = GitIgnoreTemplates
                        .FirstOrDefault(i => i?.Name.Equals("VisualStudio", StringComparison.OrdinalIgnoreCase) == true);
                });

            modelService.GetLicenses()
                .Where(x => x != null)
                .ToList()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    var sorted = x
                        .Distinct()
                        .OrderByDescending(item => item.Recommended)
                        .ThenBy(item => item.Key);
                    Licenses = new[] { LicenseItem.None }.Concat(sorted).ToList();
                });
        }

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
                    log.Error(e, "Failed to set base repository path {@0}",
                        new { localBaseRepositoryPath, BaseRepositoryPath, directory });
                }
            }, RxApp.MainThreadScheduler);
        }

        bool IsAlreadyRepoAtPath(string potentialRepositoryName)
        {
            Guard.ArgumentNotNull(potentialRepositoryName, nameof(potentialRepositoryName));

            bool isAlreadyRepoAtPath = false;
            var validationResult = BaseRepositoryPathValidator.ValidationResult;
            string safeRepositoryName = GetSafeRepositoryName(potentialRepositoryName);
            if (validationResult != null && validationResult.IsValid)
            {
                if (Path.GetInvalidPathChars().Any(chr => BaseRepositoryPath.Contains(chr)))
                    return false;
                string potentialPath = Path.Combine(BaseRepositoryPath, safeRepositoryName);
                isAlreadyRepoAtPath = operatingSystem.Directory.Exists(potentialPath);
            }
            return isAlreadyRepoAtPath;
        }

        IObservable<Unit> OnCreateRepository()
        {
            var newRepository = GatherRepositoryInfo();

            return repositoryCreationService.CreateRepository(
                newRepository,
                SelectedAccount,
                BaseRepositoryPath,
                modelService.ApiClient)
                .Do(_ => usageTracker.IncrementCounter(x => x.NumberOfReposCreated).Forget());
        }

        ReactiveCommand<Unit, Unit> InitializeCreateRepositoryCommand()
        {
            var canCreate = this.WhenAny(
                x => x.RepositoryNameValidator.ValidationResult.IsValid,
                x => x.BaseRepositoryPathValidator.ValidationResult.IsValid,
                (x, y) => x.Value && y.Value);
            var createCommand = ReactiveCommand.CreateFromObservable(OnCreateRepository, canCreate);
            createCommand.ThrownExceptions.Subscribe(ex =>
            {
                if (!Extensions.ExceptionExtensions.IsCriticalException(ex))
                {
                    log.Error(ex, "Error creating repository");
#pragma warning disable CS0618 // Type or member is obsolete
                    UserError.Throw(TranslateRepositoryCreateException(ex));
#pragma warning restore CS0618 // Type or member is obsolete
                }
            });

            return createCommand;
        }

        static string StripSurroundingQuotes(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));

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
            Guard.ArgumentNotNull(ex, nameof(ex));

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
