using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    public class SectionControl : ContentControl
    {
        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
            "Buttons",
            typeof(IList),
            typeof(SectionControl));

        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(SectionControl));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(SectionControl),
            new FrameworkPropertyMetadata(true));

        static SectionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SectionControl), new FrameworkPropertyMetadata(typeof(SectionControl)));
        }

        public SectionControl()
        {
            Buttons = new ArrayList();
        }

        public IList Buttons
        {
            get { return (IList)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
    }
}
