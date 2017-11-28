# Multi-paged Dialogs

Some dialogs will be multi-paged - for example the login dialog has a credentials page and a 2Fa page that is shown if two-factor authorization is required.

## The View Model

To help implement view models for a multi-page dialog there is a useful base class called `PagedDialogViewModelBase`. The typical way of implementing this is as follows:

- Define each page of the dialog as you would [implement a single dialog view model](implementing-a-dialog-view.md)
- Implement a "container" view model for the dialog that inherits from `PagedDialogViewModel`
- Import each page into the container view model
- Add logic to switch between pages by setting the `PagedDialogViewModelBase.Content` property
- Add a `Done` observable

Here's a simple example of a container dialog that has two pages. The pages are switched using `ReactiveCommand`s:

```csharp
using System;
using System.ComponentModel.Composition;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IExamplePagedDialogViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExamplePagedDialogViewModel : PagedDialogViewModelBase,
                                               IExamplePagedDialogViewModel
    {
        [ImportingConstructor]
        public ExamplePagedDialogViewModel(
            IPage1ViewModel page1,
            IPage2ViewModel page2)
        {
            Content = page1;
            page1.Next.Subscribe(_ => Content = page2);
            page2.Previous.Subscribe(_ => Content = page1);
            Done = Observable.Merge(page2.Done, page2.Done);
        }

        public override IObservable<object> Done { get; }
    }
}
```

## The View

The view in this case is very simple: it just needs to display the `Content` property of the container view model:

```xml
<UserControl x:Class="GitHub.VisualStudio.Views.Dialog.ExamplePagedDialogView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Content="{Binding Content}">
</UserControl>
```

```csharp
using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IExamplePagedDialogViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ExamplePagedDialogView : UserControl
    {
        public NewLoginView()
        {
            InitializeComponent();
        }
    }
}
```

