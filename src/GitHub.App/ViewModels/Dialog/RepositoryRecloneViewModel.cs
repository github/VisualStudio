using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using ReactiveUI;
using Rothko;
using Serilog;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IRepositoryRecloneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryRecloneViewModel : ViewModelBase, IRepositoryRecloneViewModel
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryRecloneViewModel>();

        readonly IOperatingSystem operatingSystem;
        readonly ReactiveCommand<Unit, Unit> browseForDirectoryCommand = ReactiveCommand.Create(() => { });
        readonly ObservableAsPropertyHelper<bool> canClone;
        string baseRepositoryPath;

        [ImportingConstructor]
        public RepositoryRecloneViewModel(
            IRepositoryCloneService cloneService,
            IOperatingSystem operatingSystem)
        {
            Guard.ArgumentNotNull(cloneService, nameof(cloneService));
            Guard.ArgumentNotNull(operatingSystem, nameof(operatingSystem));

            this.operatingSystem = operatingSystem;

            var baseRepositoryPath = this.WhenAny(
                x => x.BaseRepositoryPath,
                x => x.SelectedRepository,
                (x, y) => x.Value);

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(baseRepositoryPath)
                .IfNullOrEmpty(Resources.RepositoryCreationClonePathEmpty)
                .IfTrue(x => x.Length > 200, Resources.RepositoryCreationClonePathTooLong)
                .IfContainsInvalidPathChars(Resources.RepositoryCreationClonePathInvalidCharacters)
                .IfPathNotRooted(Resources.RepositoryCreationClonePathInvalid)
                .IfTrue(IsAlreadyRepoAtPath, Resources.RepositoryNameValidatorAlreadyExists);

            var canCloneObservable = this.WhenAny(
                x => x.SelectedRepository,
                x => x.BaseRepositoryPathValidator.ValidationResult.IsValid,
                (x, y) => x.Value != null && y.Value);
            canClone = canCloneObservable.ToProperty(this, x => x.CanClone);
            CloneCommand = ReactiveCommand.Create(() => { }, canCloneObservable);

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());
            this.WhenAny(x => x.BaseRepositoryPathValidator.ValidationResult, x => x.Value)
                .Subscribe();
            BaseRepositoryPath = cloneService.DefaultClonePath;
        }

        public Task InitializeAsync(IConnection connection)
        {
            Title = string.Format(CultureInfo.CurrentCulture, Resources.CloneTitle, connection.HostAddress.Title);
            return Task.CompletedTask;
        }

        bool IsAlreadyRepoAtPath(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));

            bool isAlreadyRepoAtPath = false;

            if (SelectedRepository != null)
            {
                string potentialPath = Path.Combine(path, SelectedRepository.Name);
                isAlreadyRepoAtPath = operatingSystem.Directory.Exists(potentialPath);
            }

            return isAlreadyRepoAtPath;
        }

        IObservable<Unit> ShowBrowseForDirectoryDialog()
        {
            return Observable.Start(() =>
            {
                // We store this in a local variable to prevent it changing underneath us while the
                // folder dialog is open.
                var localBaseRepositoryPath = BaseRepositoryPath;
                var browseResult = operatingSystem.Dialog.BrowseForDirectory(localBaseRepositoryPath, Resources.BrowseForDirectory);

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
                    log.Error(e, "Failed to set base repository path. localBaseRepositoryPath = {0} BaseRepositoryPath = {1} Chosen directory = {2}",
                        localBaseRepositoryPath ?? "(null)", BaseRepositoryPath ?? "(null)", directory ?? "(null)");
                }
            }, RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Gets the dialog title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Path to clone repositories into
        /// </summary>
        public string BaseRepositoryPath
        {
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, value); }
        }

        /// <summary>
        /// Signals that the user clicked the clone button.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CloneCommand { get; private set; }

        RepositoryModel selectedRepository;
        /// <summary>
        /// Selected repository to clone
        /// </summary>
        public RepositoryModel SelectedRepository
        {
            get { return selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref selectedRepository, value); }
        }

        public ICommand BrowseForDirectory
        {
            get { return browseForDirectoryCommand; }
        }

        public bool CanClone
        {
            get { return canClone.Value; }
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }

        public IObservable<object> Done => CloneCommand.Select(_ => BaseRepositoryPath);
    }
}
