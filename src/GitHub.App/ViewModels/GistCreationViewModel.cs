using System;
using GitHub.Exports;
using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Extensions;
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
            this.apiClient = repositoryHost.ApiClient;

            // Since the filename is required, go ahead and give it something default so the user is not forced to 
            // add a custom name if they do not want to
            FileName = Resources.DefaultGistFileName;
            selectedText = selectedTextProvider.GetSelectedText().ToProperty(this, x => x.SelectedText);

            var canCreateGist = this.WhenAny( 
                x => x.FileName,
                x => x.SelectedText,
                (fileName, selectedText) => fileName.Value.IsNotNullOrEmptyOrWhiteSpace() && selectedText.Value.IsNotNullOrEmptyOrWhiteSpace());

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

        readonly ObservableAsPropertyHelper<string> selectedText;
        public string SelectedText
        {
            [return: AllowNull]
            get { return selectedText.Value; }
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
