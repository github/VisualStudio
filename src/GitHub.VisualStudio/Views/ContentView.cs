using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Data;
using GitHub.Exports;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views
{
    /// <summary>
    /// A view that simply displays whatever is in the Content property of its DataContext.
    /// </summary>
    /// <remarks>
    /// A control which displays the Content property of a view model is commonly needed when
    /// displaying multi-page interfaces. To use this control as a view for such a purpose, 
    /// simply add an `[ExportViewFor]` attribute to this class with the type of the view model
    /// interface.
    /// </remarks>
    [ExportViewFor(typeof(ILoginViewModel))]
    [ExportViewFor(typeof(INavigationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ContentView : ContentControl
    {
        public ContentView()
        {
            BindingOperations.SetBinding(this, ContentProperty, new Binding("Content"));
        }
    }
}
