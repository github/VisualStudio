using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GitHub.Exports;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using NullGuard;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GistCreationViewModel : BaseViewModel, IGistCreationViewModel
    {
        readonly IApiClient apiClient;
        readonly ObservableAsPropertyHelper<IAccount> account;

        [ImportingConstructor]
        GistCreationViewModel(
        IConnectionRepositoryHostMap connectionRepositoryHostMap,
        ISelectedTextProvider selectedTextProvider)
        : this(connectionRepositoryHostMap.CurrentRepositoryHost, selectedTextProvider)
        {
        }

        public GistCreationViewModel(IRepositoryHost repositoryHost, ISelectedTextProvider selectedTextProvider)
        {
            Title = Resources.CreateGistTitle;
            apiClient = repositoryHost.ApiClient;

            // Since the filename is required, go ahead and give it something default so the user is not forced to 
            // add a custom name if they do not want to
            FileName = Resources.DefaultGistFileName;
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

        IObservable<Gist> OnCreateGist(object _)
        {
            var newGist = new NewGist
            {
                Description = Description,
                Public = !IsPrivate
            };
            newGist.Files.Add(FileName, SelectedText);
            return apiClient.CreateGist(newGist);
        }

        public IReactiveCommand<Gist> CreateGist { get; }
        public IAccount Account { get { return account.Value; } }

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
