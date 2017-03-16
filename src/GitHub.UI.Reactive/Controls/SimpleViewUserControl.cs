using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.UI
{
    /// <summary>
    /// Base class for all of our user controls. This one does not import GitHub resource/styles and is used by the 
    /// publish control.
    /// </summary>
    public class SimpleViewUserControl<TInterface, TImplementor> : UserControl, IViewFor<TInterface>, IView 
        where TInterface : class, IViewModel
        where TImplementor : class
    {
        public SimpleViewUserControl()
        {
            DataContextChanged += (s, e) => ViewModel = (TInterface)e.NewValue;

            this.WhenActivated(d =>
            {
                d(this.Events()
                    .KeyUp
                    .Where(x => x.Key == Key.Escape && !x.Handled)
                    .Subscribe(key =>
                    {
                        key.Handled = true;
                        (this.ViewModel as IDialogViewModel)?.Cancel.Execute(null);
                    }));
            });
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TInterface), typeof(TImplementor), new PropertyMetadata(null));

        [AllowNull]
        object IViewFor.ViewModel
        {
            [return:AllowNull]
            get { return ViewModel; }
            set { ViewModel = (TInterface)value; }
        }

        [AllowNull]
        IViewModel IView.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
        }

        [AllowNull]
        public TInterface ViewModel
        {
            [return: AllowNull]
            get { return (TInterface)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        [AllowNull]
        TInterface IViewFor<TInterface>.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = value; }
        }
    }
}
