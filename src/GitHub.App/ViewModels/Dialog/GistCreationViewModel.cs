using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using Serilog;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IGistCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GistCreationViewModel : ViewModelBase, IGistCreationViewModel
    {
        static readonly ILogger log = LogManager.ForContext<GistCreationViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        readonly IGistPublishService gistPublishService;
        readonly INotificationService notificationService;
        readonly IUsageTracker usageTracker;
        IApiClient apiClient;
        ObservableAsPropertyHelper<IAccount> account;

        [ImportingConstructor]
        public GistCreationViewModel(
            IModelServiceFactory modelServiceFactory,
            ISelectedTextProvider selectedTextProvider,
            IGistPublishService gistPublishService,
            INotificationService notificationService,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(selectedTextProvider, nameof(selectedTextProvider));
            Guard.ArgumentNotNull(gistPublishService, nameof(gistPublishService));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            this.modelServiceFactory = modelServiceFactory;
            this.gistPublishService = gistPublishService;
            this.notificationService = notificationService;
            this.usageTracker = usageTracker;

            FileName = VisualStudio.Services.GetFileNameFromActiveDocument() ?? Resources.DefaultGistFileName;
            SelectedText = selectedTextProvider.GetSelectedText();

            var canCreateGist = this.WhenAny( 
                x => x.FileName,
                fileName => !String.IsNullOrEmpty(fileName.Value));

            CreateGist = ReactiveCommand.CreateFromObservable(OnCreateGist, canCreateGist);
        }

        public async Task InitializeAsync(IConnection connection)
        {
            var modelService = await modelServiceFactory.CreateAsync(connection);
            apiClient = modelService.ApiClient;

            // This class is only instantiated after we are logged into to a github account, so we should be safe to grab the first one here as the defaut.
            account = modelService.GetAccounts()
                .FirstAsync()
                .Select(a => a.First())
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Account);
        }

        IObservable<Gist> OnCreateGist()
        {
            var newGist = new NewGist
            {
                Description = Description,
                Public = !IsPrivate
            };

            newGist.Files.Add(FileName, SelectedText);

            return gistPublishService.PublishGist(apiClient, newGist)
                .Do(_ => usageTracker.IncrementCounter(x => x.NumberOfGists).Forget())
                .Catch<Gist, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex, "Error Creating Gist");
                        var error = StandardUserErrors.GetUserFriendlyErrorMessage(ex, ErrorType.GistCreateFailed);
                        notificationService.ShowError(error);
                    }
                    return Observable.Return<Gist>(null);
                });
        }

        public string Title => Resources.CreateGistTitle;

        public ReactiveCommand<Unit, Gist> CreateGist { get; }

        public IAccount Account
        {
            get { return account.Value; }
        }

        bool isPrivate;
        public bool IsPrivate
        {
            get { return isPrivate; }
            set { this.RaiseAndSetIfChanged(ref isPrivate, value); }
        }

        string description;
        public string Description
        {
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        string selectedText;
        public string SelectedText
        {
            get { return selectedText; }
            set { this.RaiseAndSetIfChanged(ref selectedText, value); }
        } 

        string fileName;
        public string FileName
        {
            get { return fileName; }
            set { this.RaiseAndSetIfChanged(ref fileName, value); }
        }

        public IObservable<object> Done => CreateGist.Where(x => x != null);
    }
}
