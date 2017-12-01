# How ViewModels are Turned into Views

We make use of the  [MVVM pattern](https://msdn.microsoft.com/en-us/library/ff798384.aspx), in which application level code is not aware of the view level. MVVM takes advantage of the fact that `DataTemplate`s can be used to create views from view models.

## DataTemplates

[`DataTemplate`](https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/data-templating-overview)s are a WPF feature that allow you to define the presentation of your data. Consider a simple view model:

```csharp
public class ViewModel
{
  public string Greeting => "Hello World!";
}
```

And a window:

```csharp
public class MainWindow : Window
{
  public MainWindow()
  {
    DataContext = new ViewModel();
    InitializeComponent();
  }
}
```

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:MyApp"
        Content="{Binding}">
<Window>

```

Here we're binding the `Content` of the `Window` to the `Window.DataContext`, which we're setting in the constructor to be an instance of `ViewModel`.

One can choose to display the `ViewModel` instance in any way we want by using a `DataTemplate`:

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:MyApp"
        Content="{Binding}">
  <Window.Resources>
    <DataTemplate DataType="{x:Type local:ViewModel}">
      
      <!-- Display ViewModel.Greeting in a red border with rounded corners -->
	  <Border Background="Red" CornerRadius="8">
        <TextBlock Binding="{Binding Greeting}"/>
      </Border>
        
    </DataTemplate>
  </Window.Resources>
</pfui:DialogWindow>
```

This is the basis for converting view models to views.

## ViewLocator

There are currently two top-level controls for our UI:

- [GitHubDialogWindow](../src/GitHub.VisualStudio/Views/Dialog/GitHubDialogWindow.xaml) for the dialog which shows the login, clone, etc views
- [GitHubPaneView](../src/GitHub.VisualStudio/Views/GitHubPane/GitHubPaneView.xaml) for the GitHub pane

In the resources for each of these top-level controls we define a `DataTemplate` like so:

```xml
<views:ViewLocator x:Key="viewLocator"/>
<DataTemplate DataType="{x:Type vm:ViewModelBase}">
  <ContentControl Content="{Binding Converter={StaticResource viewLocator}}"/>
</DataTemplate>
```

The `DataTemplate.DataType` here applies the template to all classes inherited from [`GitHub.ViewModels.ViewModelBase`](../src/GitHub.Exports.Reactive/ViewModels/ViewModelBase.cs) [1]. The template defines a single `ContentControl` whose contents are created by a `ViewLocator`.

The [`ViewLocator`](../src/GitHub.VisualStudio/Views/ViewLocator.cs) class is an `IValueConverter` which then creates an instance of the appropriate view for the view model using MEF.

And thus a view model becomes a view.

[1]: it would be nice to make it apply to all classes that inherit `IViewModel` but unfortunately WPF's `DataTemplate`s don't work with interfaces.