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
using GitHub.App;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using GitHub.UserErrors;
using GitHub.Validation;
using Octokit;
using ReactiveUI;
using Rothko;
using GitHub.Collections;
using GitHub.Logging;
using Serilog;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCreationViewModel : RepositoryFormViewModel, IRepositoryCreationViewModel
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryCreationViewModel>();

        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ObservableAsPropertyHelper<IReadOnlyList<IAccount>> accounts;
        readonly IRepositoryHost repositoryHost;
        readonly IRepositoryCreationService repositoryCreationService;
        readonly ObservableAsPropertyHelper<bool> isCreating;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;
        readonly IOperatingSystem operatingSystem;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        RepositoryCreationViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            IOperatingSystem operatingSystem,
            IRepositoryCreationService repositoryCreationService,
            IUsageTracker usageTracker)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, operatingSystem, repositoryCreationService, usageTracker)
        {}

        public RepositoryCreationViewModel(
            IRepositoryHost repositoryHost,
            IOperatingSystem operatingSystem,
            IRepositoryCreationService repositoryCreationService,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(repositoryHost, nameof(repositoryHost));
            Guard.ArgumentNotNull(operatingSystem, nameof(operatingSystem));
            Guard.ArgumentNotNull(repositoryCreationService, nameof(repositoryCreationService));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            this.repositoryHost = repositoryHost;
            this.operatingSystem = operatingSystem;
            this.repositoryCreationService = repositoryCreationService;
            this.usageTracker = usageTracker;

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CreateTitle, repositoryHost.Title);
            SelectedGitIgnoreTemplate = GitIgnoreItem.None;
            SelectedLicense = LicenseItem.None;

            accounts = repositoryHost.ModelService.GetAccounts()
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Accounts, initialValue: new ReadOnlyCollection<IAccount>(new IAccount[] {}));

            this.WhenAny(x => x.Accounts, x => x.Value)
                .Select(accts => accts?.FirstOrDefault())
                .WhereNotNull()
                .Subscribe(a => SelectedAccount = a);

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

            GitIgnoreTemplates = TrackingCollection.CreateListenerCollectionAndRun(
                repositoryHost.ModelService.GetGitIgnoreTemplates(),
                new[] { GitIgnoreItem.None },
                OrderedComparer<GitIgnoreItem>.OrderByDescending(item => GitIgnoreItem.IsRecommended(item.Name)).Compare,
                x =>
                {
                    if (x.Name.Equals("VisualStudio", StringComparison.OrdinalIgnoreCase))
                        SelectedGitIgnoreTemplate = x;
                });

            Licenses = TrackingCollection.CreateListenerCollectionAndRun(
                repositoryHost.ModelService.GetLicenses(),
                new[] { LicenseItem.None },
                OrderedComparer<LicenseItem>.OrderByDescending(item => LicenseItem.IsRecommended(item.Name)).Compare);

            BaseRepositoryPath = repositoryCreationService.DefaultClonePath;
        }

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
        public ICommand BrowseForDirectory { get { return browseForDirectoryCommand; } }

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
        public IReactiveCommand<Unit> CreateRepository { get; private set; }

        public override IObservable<Unit> Done => CreateRepository;

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

        IObservable<Unit> OnCreateRepository(object state)
        {
            var newRepository = GatherRepositoryInfo();

            return repositoryCreationService.CreateRepository(
                newRepository,
                SelectedAccount,
                BaseRepositoryPath,
                repositoryHost.ApiClient)
                .Do(_ => usageTracker.IncrementCreateCount().Forget());
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
                    log.Error(ex, "Error creating repository");
                    UserError.Throw(TranslateRepositoryCreateException(ex));
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
