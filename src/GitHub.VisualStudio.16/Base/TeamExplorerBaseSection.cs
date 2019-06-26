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
    /// Team Explorer base section class.
    /// </summary>
    public class TeamExplorerBaseSection : TeamExplorerBase, ITeamExplorerSection
    {
        #region ITeamExplorerSection

        /// <summary>
        /// Initialize the section.
        /// </summary>
        public virtual void Initialize(object sender, SectionInitializeEventArgs e)
        {
            this.ServiceProvider = e.ServiceProvider;
        }

        /// <summary>
        /// Save context handler that is called before a section is unloaded.
        /// </summary>
        public virtual void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
        }

        /// <summary>
        /// Get/set the section title.
        /// </summary>
        public string Title
        {
            get { return m_title; }
            set { m_title = value; RaisePropertyChanged("Title"); }
        }
        private string m_title;

        /// <summary>
        /// Get/set the section content.
        /// </summary>
        public object SectionContent
        {
            get { return m_sectionContent; }
            set { m_sectionContent = value; RaisePropertyChanged("SectionContent"); }
        }
        private object m_sectionContent;

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
        /// Get/set the IsExpanded flag.
        /// </summary>
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set { m_isExpanded = value; RaisePropertyChanged("IsExpanded"); }
        }
        private bool m_isExpanded = true;

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
        /// Called when the section is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Loaded(object sender, SectionLoadedEventArgs e)
        {
        }

        /// <summary>
        /// Refresh the section contents.
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
        /// Get the requested extensibility service from the section.  Return
        /// null if the service is not offered by this section.
        /// </summary>
        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        #endregion
    }
}
