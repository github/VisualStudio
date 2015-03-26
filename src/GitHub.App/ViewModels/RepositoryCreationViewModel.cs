using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
    public class RepositoryCreationViewModel : RepositoryFormViewModel, IRepositoryCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly IRepositoryCreationService repositoryCreationService;
        readonly ObservableAsPropertyHelper<bool> isCreating;
        readonly ObservableAsPropertyHelper<bool> canKeepPrivate;

        [ImportingConstructor]
        public RepositoryCreationViewModel(IOperatingSystem operatingSystem, IRepositoryHosts hosts, IRepositoryCreationService repositoryCreationService)
            : base(operatingSystem, hosts)
        {
            this.repositoryCreationService = repositoryCreationService;

            Accounts = hosts.GitHubHost.Accounts;
            Accounts = RepositoryHost.Accounts ?? new ReactiveList<IAccount>();
            Debug.Assert(Splat.ModeDetector.InUnitTestRunner() || Accounts.Any(), "There must be at least one account");
            var selectedAccount = Accounts.FirstOrDefault();
            if (selectedAccount != null)
            {
                SelectedAccount = Accounts.FirstOrDefault();
            }

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(this.WhenAny(x => x.BaseRepositoryPath, x => x.Value))
                .IfNullOrEmpty("Please enter a repository path")
                .IfTrue(x => x.Length > 200, "Path too long")
                .IfContainsInvalidPathChars("Path contains invalid characters")
                .IfPathNotRooted("Please enter a valid path");

            var nonNullRepositoryName = this.WhenAny(
                x => x.RepositoryName,
                x => x.BaseRepositoryPath,
                (x, y) => x.Value)
                .WhereNotNull();

            RepositoryNameValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .IfNullOrEmpty("Please enter a repository name")
                .IfTrue(x => x.Length > 100, "Repository name must be fewer than 100 characters")
                .IfTrue(x => IsAlreadyRepoAtPath(GetSafeRepositoryName(x)), "Repository with same name already exists at this location");

            SafeRepositoryNameWarningValidator = ReactivePropertyValidator.ForObservable(nonNullRepositoryName)
                .Add(repoName =>
                {
                    var parsedReference = GetSafeRepositoryName(repoName);
                    return parsedReference != repoName ? "Will be created as " + parsedReference : null;
                });

            CreateRepository = InitializeCreateRepositoryCommand();

            canKeepPrivate = CanKeepPrivateObservable.CombineLatest(CreateRepository.IsExecuting,
                (canKeep, publishing) => canKeep && !publishing)
                .ToProperty(this, x => x.CanKeepPrivate);

            isCreating = CreateRepository.IsExecuting
                .ToProperty(this, x => x.IsCreating);
        }

        public IReactiveCommand<Unit> CreateRepository { get; private set; }

        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

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

        public bool IsCreating
        {
            get { return isCreating.Value; }
        }

        public bool CanKeepPrivate
        {
            get { return canKeepPrivate.Value; }
        }

        public ReactiveList<IAccount> Accounts
        {
            get;
            private set;
        }

        IObservable<Unit> ShowBrowseForDirectoryDialog()
        {
            return Observable.Start(() =>
            {
                // We store this in a local variable to prevent it changing underneath us while the
                // folder dialog is open.
                var localBaseRepositoryPath = BaseRepositoryPath;
                var browseResult = OperatingSystem.Dialog.BrowseForDirectory(localBaseRepositoryPath,
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
                return OperatingSystem.File.Exists(Path.Combine(path, ".git", "HEAD"));
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
    }
}
