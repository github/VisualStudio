using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;

namespace GitHub.UI
{
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "GitHub")]
    public class GitHubTabControl : TabControl
    {
        FrameworkElement highlightElement;
        TranslateTransform highlightTransform;

        private Duration duration;
        private DoubleAnimation positionAnimation;
        private DoubleAnimation widthAnimation;

        // Disabling animation for now, see 0df5fb0e2 for justification
        public static bool IsAnimationEnabled { get; set; }

        public GitHubTabControl()
        {
            if (IsAnimationEnabled)
            {
                Loaded += (sender, args) =>
                {
                    highlightElement = Template.FindName("TabHighlight", this) as FrameworkElement;
                    Debug.Assert(highlightElement != null, "The template 'TabHighlight' is null or not a FrameworkElement");
                    highlightTransform = Template.FindName("TabHighlightTransform", this) as TranslateTransform;
                    Debug.Assert(highlightTransform != null, "The template 'TabHighlightTransform' is null or not a FrameworkElement");
                    highlightElement.Visibility = Visibility.Visible;
                    UpdateHighlight();
                };

                duration = new Duration(TimeSpan.FromSeconds(0.1));
                positionAnimation = new DoubleAnimation { Duration = duration };
                widthAnimation = new DoubleAnimation { Duration = duration };

                this.WhenAny(x => x.SelectedItem, x => x.Value)
                    .Subscribe(_ => UpdateHighlight());
            }
        }

        void UpdateHighlight()
        {
            if (highlightElement == null || SelectedIndex < 0 || highlightTransform == null) return;

            SetHighlightedTabInfo(SelectedItem);

            AnimateHighlightPosition(highlightTransform, positionAnimation);
            AnimateHighlightWidth(highlightElement, widthAnimation);
        }

        public static readonly DependencyProperty SelectedTabHeaderPositionProperty =
            DependencyProperty.Register("SelectedTabHeaderPosition", typeof(Point), typeof(GitHubTabControl),
            new PropertyMetadata(default(Point)));

        public Point SelectedTabHeaderPosition
        {
            get { return (Point)GetValue(SelectedTabHeaderPositionProperty); }
            private set { SetValue(SelectedTabHeaderPositionProperty, value); }
        }

        public static readonly DependencyProperty SelectedTabContentWidthProperty =
            DependencyProperty.Register("SelectedTabHeaderContentWidth", typeof(double), typeof(GitHubTabControl),
            new PropertyMetadata(default(double)));

        public double SelectedTabHeaderContentWidth
        {
            get { return (double)GetValue(SelectedTabContentWidthProperty); }
            private set { SetValue(SelectedTabContentWidthProperty, value); }
        }

        //UIElement GetTabHeaderFromSelectedItem()
        //{
        //}

        void SetHighlightedTabInfo(object selectedItem)
        {
            var selectedTab = ItemContainerGenerator.ContainerFromItem(selectedItem) as TabItem;
            if (selectedTab == null) return;

            var tabHeaderContent = selectedTab.Template.FindName("ContentSite", selectedTab) as FrameworkElement;
            if (tabHeaderContent == null) return;

            SelectedTabHeaderContentWidth = tabHeaderContent.ActualWidth;

            var headerPanel = Template.FindName("HeaderPanel", this) as UIElement;
            if (headerPanel == null) return;
            SelectedTabHeaderPosition = tabHeaderContent.TranslatePoint(new Point(0, 0), headerPanel);
        }

        void AnimateHighlightWidth(FrameworkElement highlightFrameworkElement, DoubleAnimation widthDoubleAnimation)
        {
            if (highlightFrameworkElement == null) return;

            widthDoubleAnimation.From = highlightFrameworkElement.ActualWidth;
            widthDoubleAnimation.To = SelectedTabHeaderContentWidth;
            highlightFrameworkElement.BeginAnimation(WidthProperty, widthDoubleAnimation);
        }

        void AnimateHighlightPosition(
            TranslateTransform highlightTranslateTransform,
            DoubleAnimation positionDoubleAnimation)
        {
            if (highlightTranslateTransform == null) return;

            positionDoubleAnimation.From = highlightTranslateTransform.X;
            positionDoubleAnimation.To = SelectedTabHeaderPosition.X;

            highlightTranslateTransform.BeginAnimation(TranslateTransform.XProperty, positionDoubleAnimation);
        }
    }
}