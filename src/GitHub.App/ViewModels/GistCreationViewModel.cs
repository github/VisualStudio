using System;
using GitHub.Exports;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using NullGuard;
using GitHub.Extensions;
using NLog;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GistCreationViewModel : BaseViewModel, IGistCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IApiClient apiClient;
        readonly ObservableAsPropertyHelper<IAccount> account;
        readonly IGistPublishService gistPublishService;
        readonly INotificationService notificationService;

        [ImportingConstructor]
        GistCreationViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ISelectedTextProvider selectedTextProvider,
            IGistPublishService gistPublishService,
            INotificationService notificationService)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, selectedTextProvider, gistPublishService)
        {
            this.notificationService = notificationService;
        }

        public GistCreationViewModel(
            IRepositoryHost repositoryHost,
            ISelectedTextProvider selectedTextProvider,
            IGistPublishService gistPublishService)
        {
            Title = Resources.CreateGistTitle;
            apiClient = repositoryHost.ApiClient;
            this.gistPublishService = gistPublishService;

            FileName = VisualStudio.Services.GetFileNameFromActiveDocument() ?? Resources.DefaultGistFileName;
            SelectedText = selectedTextProvider.GetSelectedText();

            // This class is only instantiated after we are logged into to a github account, so we should be safe to grab the first one here as the defaut.
            account = repositoryHost.ModelService.GetAccounts()
                .FirstAsync()
                .Select(a => a.First())
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Account);

            var canCreateGist = this.WhenAny( 
                x => x.FileName,
                fileName => !String.IsNullOrEmpty(fileName.Value));

            CreateGist = ReactiveCommand.CreateAsyncObservable(canCreateGist, OnCreateGist);
        }

        IObservable<ProgressState> OnCreateGist(object unused)
        {
            var newGist = new NewGist
            {
                Description = Description,
                Public = !IsPrivate
            };

            newGist.Files.Add(FileName, SelectedText);

            return gistPublishService.PublishGist(apiClient, newGist)
                .Select(_ => ProgressState.Success)
                .Catch<ProgressState, Exception>(ex =>
                {
                    if (!ex.IsCriticalException())
                    {
                        log.Error(ex);
                        var error = StandardUserErrors.GetUserFriendlyErrorMessage(ex, ErrorType.GistCreateFailed);
                        notificationService.ShowError(error);
                    }
                    return Observable.Return(ProgressState.Fail);
                });
        }

        public IReactiveCommand<ProgressState> CreateGist { get; }

        public IAccount Account
        {
            [return: AllowNull]
            get { return account.Value; }
        }

        bool isPrivate;
        public bool IsPrivate
        {
            get { return isPrivate; }
            set { this.RaiseAndSetIfChanged(ref isPrivate, value); }
        }

        string description;
        [AllowNull]
        public string Description
        {
            [return: AllowNull]
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        string selectedText;
        [AllowNull]
        public string SelectedText
        {
            [return: AllowNull]
            get { return selectedText; }
            set { this.RaiseAndSetIfChanged(ref selectedText, value); }
        } 

        string fileName;
        [AllowNull]
        public string FileName
        {
            [return: AllowNull]
            get { return fileName; }
            set { this.RaiseAndSetIfChanged(ref fileName, value); }
        }
    }
}
