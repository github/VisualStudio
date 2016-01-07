using System;
using GitHub.Exports;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
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
        readonly ISelectedTextProvider selectedTextProvider;
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
            this.selectedTextProvider = selectedTextProvider;
            this.apiClient = repositoryHost.ApiClient;

            // Since the filename is required, go ahead and give it something default so the user is not forced to 
            // add a custom name if they do not want to
            FileName = Resources.DefaultGistFileName;

            var canCreateGist = this.WhenAny( 
                x => x.FileName,
                fileName => !string.IsNullOrEmpty(fileName.Value));

            CreateGist = ReactiveCommand.CreateAsyncObservable(canCreateGist, OnCreateGist);
        }

        private IObservable<Gist> OnCreateGist(object _)
        {
            return selectedTextProvider.GetSelectedText().Select(selectedText =>
            {
                var newGist = new NewGist
                {
                    Description = Description,
                    Public = !IsPrivate
                };
                newGist.Files.Add(FileName, selectedText);
                return apiClient.CreateGist(newGist);
            }).SelectMany(gists => gists);
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
