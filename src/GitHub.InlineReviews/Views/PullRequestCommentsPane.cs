//------------------------------------------------------------------------------
// <copyright file="PullRequestCommentsToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace GitHub.InlineReviews.Views
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("aa280a78-f2fa-49cd-b2f9-21426b40501f")]
    public class PullRequestCommentsPane : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestCommentsPane"/> class.
        /// </summary>
        public PullRequestCommentsPane() : base(null)
        {
            this.Caption = "Pull Request Comments";
            this.Content = new PullRequestCommentsView();
        }
    }
}
