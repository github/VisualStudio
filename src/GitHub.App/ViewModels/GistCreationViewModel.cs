using System;
using GitHub.Exports;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.TextManager.Interop;
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
        GistCreationViewModel(ISelectedTextProvider selectedTextProvider, IApiClientFactory apiClientFactory)
        {
            Title = Resources.CreateGistTitle;
            this.selectedTextProvider = selectedTextProvider;
            this.apiClient = apiClientFactory.Create(HostAddress.GitHubDotComHostAddress);

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

        string content;
        [AllowNull]
        public string Content
        {
            [return: AllowNull]
            get { return content; }
            set { this.RaiseAndSetIfChanged(ref content, value); }
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
