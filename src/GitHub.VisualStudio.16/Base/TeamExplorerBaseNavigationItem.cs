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
    /// Team Explorer base navigation item class.
    /// </summary>
    public class TeamExplorerBaseNavigationItem : TeamExplorerBase, ITeamExplorerNavigationItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TeamExplorerBaseNavigationItem(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        #region ITeamExplorerNavigationItem

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
        /// Get/set the item image.
        /// </summary>
        public System.Drawing.Image Image
        {
            get { return m_image; }
            set { m_image = value; RaisePropertyChanged("Image"); }
        }
        private System.Drawing.Image m_image;

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
        /// Invalidate the item state.
        /// </summary>
        public virtual void Invalidate()
        {
        }

        /// <summary>
        /// Execute the item action.
        /// </summary>
        public virtual void Execute()
        {
        }

        #endregion
    }
}
