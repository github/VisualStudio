using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.UI
{
    /// <summary>
    /// Base class for views.
    /// </summary>
    public class ViewBase<TInterface, TImplementor> : UserControl, IViewFor<TInterface>
        where TInterface : class, IViewModel
        where TImplementor : class
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TInterface), typeof(TImplementor), new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBase{TInterface, TImplementor}"/> class.
        /// </summary>
        public ViewBase()
        {
            DataContextChanged += (s, e) => ViewModel = (TInterface)e.NewValue;
            this.WhenAnyValue(x => x.ViewModel).Subscribe(x => DataContext = x);
        }

        /// <summary>
        /// Gets or sets the control's data context as a typed view model.
        /// </summary>
        public TInterface ViewModel
        {
            get { return (TInterface)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the control's data context as a typed view model. Required for interaction
        /// with ReactiveUI.
        /// </summary>
        TInterface IViewFor<TInterface>.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value; }
        }

        /// <summary>
        /// Gets or sets the control's data context. Required for interaction with ReactiveUI.
        /// </summary>
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TInterface)value; }
        }

        /// <summary>
        ///  Add an automation peer to views and custom controls 
        ///  They do not have automation peers or properties by default
        ///  https://stackoverflow.com/questions/30198109/automationproperties-automationid-on-custom-control-not-exposed
        /// </summary>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UIElementAutomationPeer(this);
        }
    }
}
