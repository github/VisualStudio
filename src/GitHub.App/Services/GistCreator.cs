using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using Octokit;
using GitHub.Factories;
using GitHub.Primitives;

namespace GitHub.Services
{
    [Export(typeof(IGistCreator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GistCreator : IGistCreator
    {
        readonly ISelectedTextProvider selectedTextProvider;
        readonly IConnectionManager connectionManager;
        readonly IRepositoryHosts repositoryHosts;
        readonly IUIProvider uiProvider;
        readonly IApiClient apiClient;

        [ImportingConstructor]
        public GistCreator(ISelectedTextProvider selectedTextProvider, IConnectionManager connectionManager, 
            IRepositoryHosts repositoryHosts, IUIProvider uiProvider, IApiClientFactory apiClientFactory)
        {
            this.selectedTextProvider = selectedTextProvider;
            this.connectionManager = connectionManager;
            this.repositoryHosts = repositoryHosts;
            this.uiProvider = uiProvider;
            this.apiClient = apiClientFactory.Create(HostAddress.GitHubDotComHostAddress);
        }

        public async Task<Gist> CreateFromSelectedText()
        {
            await EnsureLoggedIn();

            var selectedText = selectedTextProvider.GetSelectedText().ToTask().Result;

            // Show the Create Gist Window, and on Create button clicked execute the following
            return await apiClient.CreateGist("NameWillBeEnteredInThePopup", true, selectedText)
                .Catch<Gist, Octokit.NotFoundException>(_ => Observable.Return<Gist>(null))
                .ToTask();
        }

        async Task EnsureLoggedIn()
        {
            var loggedIn = await connectionManager.IsLoggedIn(repositoryHosts);

            if (!loggedIn)
                loggedIn = await Login();

            if (!loggedIn)
                throw new InvalidOperationException("cannot create a gist if the user is not authenticated");
        }

        async Task<bool> Login()
        {
            var loggedInComplete = new TaskCompletionSource<bool>();

            var authUI = uiProvider.SetupUI(UIControllerFlow.Authentication, null);
            authUI.Subscribe(_ => { }, async () =>
            {
                var isLoggedIn = await connectionManager.IsLoggedIn(repositoryHosts);
                loggedInComplete.SetResult(isLoggedIn);
            });
            uiProvider.RunUI();

            return await loggedInComplete.Task;
        }
    }
}
