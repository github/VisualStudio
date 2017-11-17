//------------------------------------------------------------------------------
// <copyright file="PullRequestCommentsToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.Views
{
    [Guid("aa280a78-f2fa-49cd-b2f9-21426b40501f")]
    public class PullRequestCommentsPane : ToolWindowPane
    {
        readonly PullRequestCommentsView view;
        IPullRequestSession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestCommentsPane"/> class.
        /// </summary>
        public PullRequestCommentsPane() : base(null)
        {
            this.Caption = "Pull Request Comments";
            this.Content = view = new PullRequestCommentsView();
        }

        public async Task Initialize(
            IPullRequestSession pullRequestSession,
            IApiClient apiClient)
        {
            Guard.ArgumentNotNull(pullRequestSession, nameof(pullRequestSession));
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));

            if (this.session != null)
                return;

            this.session = pullRequestSession;

            var viewModel = new PullRequestCommentsViewModel(pullRequestSession);
            await viewModel.Initialize();
            view.DataContext = viewModel;
        }
    }
}
