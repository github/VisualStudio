using GitHub.VisualStudio;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
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
                            var binding = GetBinding();
                            Debug.Assert(binding != null, "Cannot find ItemsSource binding. Did you configure it in the target Listbox?");
                            if (binding == null)
                                return;
                            defaultValue = PropertyPathHelper.GetValue(binding.ResolvedSource, binding.ResolvedSourcePropertyName);
                        }
                        dynamic items = AssociatedObject.ItemsSource;
                        var list = items as IList;
                        Debug.Assert(list != null, "ItemsSource data source is not an IList, cannot change it.");
                        if (list == null)
                            return;
                        try
                        {
                            items.Insert(0, defaultValue);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            throw ex;
#else
                            VsOutputLogger.WriteLine(String.Format(CultureInfo.CurrentCulture, "Could not add default empty item to the bound ItemsSource data source - the collection does not allow insertion (or the type does not match). {0}", ex));
#endif
                        }
                    }
                    else if (AssociatedObject.SelectedIndex == 0)
                    {
                        dynamic items = AssociatedObject.ItemsSource;
                        var list = items as IList;
                        Debug.Assert(list != null, "ItemsSource data source is not an IList, cannot change it.");
                        Debug.Assert(list.Count == 0, "ItemsSource data source is empty, something went wrong.");
                        if (list == null || list.Count == 0)
                            return;
                        if (items[0] == defaultValue)
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

        BindingExpression GetBinding()
        {
            var binding = AssociatedObject.GetBindingExpression(ListBox.ItemsSourceProperty);
            if (binding == null)
            {
                var m = BindingOperations.GetMultiBindingExpression(AssociatedObject, ItemsControl.ItemsSourceProperty);
                if (m != null && m.BindingExpressions.Count > 0)
                    binding = (BindingExpression) m.BindingExpressions[0];
            }
            return binding;
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
