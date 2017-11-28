# Implementing a Dialog View

GitHub for Visual Studio has a common dialog which is used to show the login, clone, create repository etc. operations. To add a new view to the dialog and show the dialog with this view, you need to do the following:

## Create a View Model and Interface

> TODO: `NewViewModelBase` will become simply `ViewModelBase` once the MVVM refactor is completed.

- Create an interface for the view model that implements `IDialogContentViewModel` in `GitHub.Exports.Reactive\ViewModels\Dialog`
- Create a view model that inherits from `NewViewModelBase` and implements the interface in `GitHub.App\ViewModels\Dialog`
- Export the view model with the interface as the contract and add a `[PartCreationPolicy(CreationPolicy.NonShared)]` attribute

A minimal example that just exposes a command that will dismiss the dialog:

```csharp
using System;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    public interface IExampleDialogViewModel : IDialogContentViewModel
    {
        ReactiveCommand<object> Dismiss { get; }
    }
}
```

```csharp
using System;
using System.ComponentModel.Composition;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    [Export(typeof(IExampleDialogViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExampleDialogViewModel : NewViewModelBase, IExampleDialogViewModel
    {
        [ImportingConstructor]
        public ExampleDialogViewModel()
        {
            Dismiss = ReactiveCommand.Create();
        }

        public string Title => "Example Dialog";

        public ReactiveCommand<object> Dismiss { get; }

        public IObservable<object> Done => Dismiss;
    }
}
```

## Create a View

> TODO: Decide if `GitHub.VisualStudio\Views` is the best place for views

- Create a WPF `UserControl` under `GitHub.VisualStudio\Views\Dialog` 
- Add an `ExportViewFor` attribute with the type of the view model interface
- Add a `PartCreationPolicy(CreationPolicy.NonShared)]` attribute

Continuing the example above:

```xml
<UserControl x:Class="GitHub.VisualStudio.Views.Dialog.ExampleDialogView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Button Command="{Binding Dismiss}" HorizontalAlignment="Center" VerticalAlignment="Center">
    Dismiss
  </Button>
</UserControl>
```

```csharp
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IExampleDialogViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ExampleDialogView : UserControl
    {
        public ExampleDialogView()
        {
            InitializeComponent();
        }
    }
}
```

## Show the Dialog!

To show the dialog you will need an instance of the `IShowDialog` service. Once you have that, simply call the `Show` method with an instance of your view model.

```csharp
var viewModel = new ExampleDialogViewModel();
showDialog.Show(viewModel)
```

## Optional: Add a method to `DialogService`

Creating a view model like this may be the right thing to do, but it's not very reusable or testable. If you want your dialog to be easy reusable, add a method to `DialogService`:

```csharp
public async Task ShowExampleDialog()
{
    var viewModel = factory.CreateViewModel<IExampleDialogViewModel>();
    await showDialog.Show(viewModel);
}
```

Obviously, add this method to `IDialogService` too.

Note that these methods are `async` - this means that if you need to do asynchronous initialization of your view model, you can do it here before calling `showDialog`.