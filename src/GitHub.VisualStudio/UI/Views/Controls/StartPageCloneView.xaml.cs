using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;
using GitHub.Services;
using System.Linq;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericStartPageCloneView : ViewBase<IBaseCloneViewModel, StartPageCloneView>
    {}

    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.StartPageClone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class StartPageCloneView : GenericStartPageCloneView
    {
        public StartPageCloneView()
        {
            InitializeComponent();

            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
