/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/
using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Team Explorer base navigation link class.
    /// </summary>
    public class TeamExplorerBaseNavigationLink : TeamExplorerBase, ITeamExplorerNavigationLink
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TeamExplorerBaseNavigationLink(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        #region ITeamExplorerNavigationLink

        /// <summary>
        /// Get/set the item text.
        /// </summary>
        public string Text
        {
            get { return m_text; }
            set { m_text = value; RaisePropertyChanged("Text"); }
        }
        private string m_text;

        /// <summary>
        /// Get/set the IsEnabled flag.
        /// </summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set { m_isEnabled = value; RaisePropertyChanged("IsEnabled"); }
        }
        private bool m_isEnabled = true;

        /// <summary>
        /// Get/set the IsVisible flag.
        /// </summary>
        public bool IsVisible
        {
            get { return m_isVisible; }
            set { m_isVisible = value; RaisePropertyChanged("IsVisible"); }
        }
        private bool m_isVisible = true;

        /// <summary>
        /// Invalidate the link state.
        /// </summary>
        public virtual void Invalidate()
        {
        }

        /// <summary>
        /// Execute the link action.
        /// </summary>
        public virtual void Execute()
        {
        }

        #endregion
    }
}
