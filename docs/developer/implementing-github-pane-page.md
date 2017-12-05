# Implementing a GitHub Pane Page

The GitHub pane displays GitHub-specific functionality in a dockable pane. To add a new page to the GitHub pane, you need to do the following:

## Create a View Model and Interface

- Create an interface for the view model that implements `IPanePageViewModel` in `GitHub.Exports.Reactive\ViewModels\GitHubPane`
- Create a view model that inherits from `PanePageViewModelBase` and implements the interface in `GitHub.App\ViewModels\GitHubPane`
- Export the view model with the interface as the contract and add a `[PartCreationPolicy(CreationPolicy.NonShared)]` attribute

A minimal example that just exposes a command that will navigate to the pull request list:

```csharp
using System;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IExamplePaneViewModel : IPanePageViewModel
    {
        ReactiveCommand<object> GoToPullRequests { get; }
    }
}
```

```csharp
using System;
using System.ComponentModel.Composition;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IExamplePaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExamplePaneViewModel : PanePageViewModelBase, IExamplePaneViewModel
    {
        [ImportingConstructor]
        public ExamplePaneViewModel()
        {
            GoToPullRequests = ReactiveCommand.Create();
            GoToPullRequests.Subscribe(_ => NavigateTo("/pulls"));
        }

        public ReactiveCommand<object> GoToPullRequests { get; }
    }
}
```

## Create a View

- Create a WPF `UserControl` under `GitHub.VisualStudio\ViewsGitHubPane` 
- Add an `ExportViewFor` attribute with the type of the view model interface
- Add a `PartCreationPolicy(CreationPolicy.NonShared)]` attribute

Continuing the example above:

```xml
<UserControl x:Class="GitHub.VisualStudio.Views.GitHubPane.ExamplePaneView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Button Command="{Binding GoToPullRequests}"
          HorizontalAlignment="Center"
          VerticalAlignment="Center">
    Go to Pull Requests
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

## Add a Route to GitHubPaneViewModel

Now you need to add a route to the `GitHubPaneViewModel`. To add a route, you must do two things:

- Add a method to `GitHubPaneViewModel`
- Add a URL handler to `GitHubPaneViewModel.NavigateTo`

So lets add the `ShowExample` method to `GitHubPaneViewModel`:

```csharp
public Task ShowExample()
{
    return NavigateTo<IExamplePaneViewModel>(x => Task.CompletedTask);
}
```
Here we call `NavigateTo` with the type of the interface of our view model. We're passing a lambda that simply returns `Task.CompletedTask` as the parameter: usually here you'd call an async initialization method on the view model, but since we don't have one in our simple example we just return a completed task.

Next we add a URL handler: our URL is going to be `github://pane/example` so we need to add a route that checks that the URL's `AbsolutePath` is `/example` and if so call the method we added above. This code is added to `GitHubPaneViewModel.NavigateTo`:

```csharp
else if (uri.AbsolutePath == "/example")
{
    await ShowExample();
}
```

For the sake of the example, we're going to show our new page as soon as the GitHub Pane is shown and the user is logged-in with an open repository. To do this, simply change `GitHubPaneViewModel.ShowDefaultPage` to the following:

```csharp
public Task ShowDefaultPage() => ShowExample();
```

When you run the extension and show the GitHub pane, our new example page should be shown. Clicking on the button in the page will navigate to the pull request list.