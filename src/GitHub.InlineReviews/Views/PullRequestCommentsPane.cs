//------------------------------------------------------------------------------
// <copyright file="PullRequestCommentsToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace GitHub.InlineReviews.Views
{
    using System;
    using System.Runtime.InteropServices;
    using Extensions;
    using Factories;
    using GitHub.Services;
    using Microsoft.VisualStudio.Shell;
    using Primitives;
    using ViewModels;

    [Guid("aa280a78-f2fa-49cd-b2f9-21426b40501f")]
    public class PullRequestCommentsPane : ToolWindowPane
    {
        readonly PullRequestCommentsView view;
        IApiClientFactory apiClientFactory;
        IPullRequestReviewSessionManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestCommentsPane"/> class.
        /// </summary>
        public PullRequestCommentsPane() : base(null)
        {
            this.Caption = "Pull Request Comments";
            this.Content = view = new PullRequestCommentsView();
        }

        public void Initialize(
            IPullRequestReviewSessionManager manager,
            IApiClientFactory apiClientFactory)
        {
            Guard.ArgumentNotNull(manager, nameof(manager));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));

            if (this.manager != null)
                return;

            this.manager = manager;
            this.apiClientFactory = apiClientFactory;

            manager.SessionChanged.Subscribe(session =>
            {
                if (session != null)
                {
                    var apiClient = apiClientFactory.Create(HostAddress.Create(session.Repository.CloneUrl));
                    var viewModel = new PullRequestCommentsViewModel(apiClient, session);
                    view.DataContext = viewModel;
                }
            });
        }
    }
}
