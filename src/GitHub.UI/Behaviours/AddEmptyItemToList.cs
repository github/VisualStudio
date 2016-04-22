using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace GitHub.UI
{
    public class AddEmptyItemToList : Behavior<ListBox>
    {
        object previousSelection = null;
        dynamic defaultValue;
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    if (previousSelection == null)
                    {
                        previousSelection = AssociatedObject.SelectedItem;
                        if (defaultValue == null)
                        {
                            var binding = AssociatedObject.GetBindingExpression(ListBox.ItemsSourceProperty);
                            if (binding == null)
                            {
                                var m = BindingOperations.GetMultiBindingExpression(AssociatedObject, ListBox.ItemsSourceProperty);
                                binding = (BindingExpression)m.BindingExpressions[0];
                            }
                            defaultValue = PropertyPathHelper.GetValue(binding.ResolvedSource, binding.ResolvedSourcePropertyName);
                        }
                        dynamic items = AssociatedObject.ItemsSource;
                        items.Insert(0, defaultValue);
                    }
                    else if (AssociatedObject.SelectedIndex == 0)
                    {
                        dynamic items = AssociatedObject.ItemsSource;
                        items.RemoveAt(0);
                    }
                }
                if (e.RemovedItems.Count > 0)
                {
                    if (e.RemovedItems[0] == defaultValue)
                    {
                        previousSelection = null;
                        AssociatedObject.SelectedItem = null;
                    }
                }
            };
        }

        static class PropertyPathHelper
        {
            public static object GetValue(object obj, string propertyPath)
            {
                var binding = new Binding(propertyPath);
                binding.Mode = BindingMode.OneTime;
                binding.Source = obj;
                BindingOperations.SetBinding(dummy, Dummy.ValueProperty, binding);
                return dummy.GetValue(Dummy.ValueProperty);
            }

            static readonly Dummy dummy = new Dummy();

            class Dummy : DependencyObject
            {
                public static readonly DependencyProperty ValueProperty =
                    DependencyProperty.Register("Value", typeof(object), typeof(Dummy), new UIPropertyMetadata(null));
            }
        }
    }
}
