/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/
using System;
using System.ComponentModel;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Team Explorer page base class.
    /// </summary>
    public class TeamExplorerBasePage : TeamExplorerBase, ITeamExplorerPage
    {
        #region ITeamExplorerPage

        /// <summary>
        /// Initialize the page.
        /// </summary>
        public virtual void Initialize(object sender, PageInitializeEventArgs e)
        {
            this.ServiceProvider = e.ServiceProvider;
        }

        /// <summary>
        /// Loaded handler that is called once the page and all sections
        /// have been initialized.
        /// </summary>
        public virtual void Loaded(object sender, PageLoadedEventArgs e)
        {
        }

        /// <summary>
        /// Save context handler that is called before a page is unloaded.
        /// </summary>
        public virtual void SaveContext(object sender, PageSaveContextEventArgs e)
        {
        }

        /// <summary>
        /// Get/set the page title.
        /// </summary>
        public string Title
        {
            get { return m_title; }
            set { m_title = value; RaisePropertyChanged("Title"); }
        }
        private string m_title;

        /// <summary>
        /// Get/set the page content.
        /// </summary>
        public object PageContent
        {
            get { return m_pageContent; }
            set { m_pageContent = value; RaisePropertyChanged("PageContent"); }
        }
        private object m_pageContent;

        /// <summary>
        /// Get/set the IsBusy flag.
        /// </summary>
        public bool IsBusy
        {
            get { return m_isBusy; }
            set { m_isBusy = value; RaisePropertyChanged("IsBusy"); }
        }
        private bool m_isBusy = false;

        /// <summary>
        /// Refresh the page contents.
        /// </summary>
        public virtual void Refresh()
        {
        }

        /// <summary>
        /// Cancel any running operations.
        /// </summary>
        public virtual void Cancel()
        {
        }

        /// <summary>
        /// Get the requested extensibility service from the page.  Return
        /// null if the service is not offered by this page.
        /// </summary>
        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        #endregion
    }
}
