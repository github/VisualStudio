using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using GitHub.ViewModels;

namespace GitHub.Factories
{
    /// <summary>
    /// Factory for creating views and view models.
    /// </summary>
    public interface IViewViewModelFactory
    {
        /// <summary>
        /// Creates a view model based on the specified interface type.
        /// </summary>
        /// <typeparam name="T">The view model interface type.</typeparam>
        /// <returns>The view model.</returns>
        TViewModel CreateViewModel<TViewModel>() where TViewModel : IViewModel;

        /// <summary>
        /// Creates a view based on a view model interface type.
        /// </summary>
        /// <typeparam name="TViewModel">The view model interface type.</typeparam>
        /// <returns>The view.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        FrameworkElement CreateView<TViewModel>() where TViewModel : IViewModel;

        /// <summary>
        /// Creates a view based on a view model interface type.
        /// </summary>
        /// <param name="viewModel">The view model interface type.</param>
        /// <returns>The view.</returns>
        FrameworkElement CreateView(Type viewModel);
    }
}
