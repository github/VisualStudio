using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views
{
    public class ExpandoLabelsView : FrameworkElement
    {
        const double LabelWidth = 16;
        const double LabelHeight = 4;
        const double LabelCornerRadius = 2;

        public static readonly DependencyProperty ViewModelsProperty =
            DependencyProperty.Register(
                nameof(ViewModels),
                typeof(IReadOnlyList<ILabelViewModel>),
                typeof(ExpandoLabelsView),
                new FrameworkPropertyMetadata(HandleViewModelsChanged));

        static readonly ControlTemplate toolTipTemplate;

        const string TooltipStyleXaml = @"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:local='clr-namespace:GitHub.VisualStudio.Views;assembly=GitHub.VisualStudio'
       TargetType='ToolTip'>
    <Setter Property='Template'>
        <Setter.Value>
            <ControlTemplate TargetType='ToolTip'>
                <local:LabelView/>
            </ControlTemplate>
        </Setter.Value>
    </Setter>-->
    <!--<Setter Property='ToolTip.Placement' Value='Top'/>
    <Setter Property='ToolTip.VerticalOffset' Value='{Binding PlacementTarget.ActualHeight, RelativeSource={RelativeSource Self}}'/>-->
</Style>";

        ToolTip toolTip;

        static ExpandoLabelsView()
        {
            toolTipTemplate = (ControlTemplate)XamlReader.Parse(@"
<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                 xmlns:local='clr-namespace:GitHub.VisualStudio.Views;assembly=GitHub.VisualStudio'
                 TargetType='ToolTip'>
    <local:LabelView/>
</ControlTemplate>
");
        }

        public ExpandoLabelsView()
        {
            toolTip = new ToolTip
            {
                Content = new LabelView(),
                Placement = PlacementMode.Top,
                PlacementTarget = this,
                Template = toolTipTemplate,
            };

            Resources.Add(
                SystemParameters.ToolTipPopupAnimationKey,
                PopupAnimation.Slide);
        }

        public IReadOnlyList<ILabelViewModel> ViewModels
        {
            get { return (IReadOnlyList<ILabelViewModel>)GetValue(ViewModelsProperty); }
            set { SetValue(ViewModelsProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var count = ViewModels?.Count ?? 0;
            return new Size(LabelWidth * count, count != 0 ? LabelHeight : 0);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ViewModels?.Count > 0)
            {
                var p = e.GetPosition(this);

                if (p.Y < LabelHeight)
                {
                    var index = (int)(p.X / LabelWidth);

                    if (index < ViewModels.Count)
                    {
                        toolTip.DataContext = ViewModels[index];
                        toolTip.HorizontalOffset = index * LabelWidth;
                        toolTip.VerticalOffset = LabelHeight;
                        toolTip.IsOpen = true;
                        return;
                    }
                }
            }

            toolTip.IsOpen = false;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            toolTip.IsOpen = false;
            base.OnMouseLeave(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ViewModels?.Count > 0)
            {
                var x = 0.0;

                foreach (var label in ViewModels)
                {
                    drawingContext.DrawRoundedRectangle(
                        label.BackgroundBrush,
                        null,
                        new Rect(x, 0, LabelWidth, LabelHeight),
                        LabelCornerRadius,
                        LabelCornerRadius);
                    x += LabelWidth;
                }
            }
        }

        void ViewModelsChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldValue = e.OldValue as INotifyCollectionChanged;
            var newValue = e.NewValue as INotifyCollectionChanged;

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= ViewModelsCollectionChanged;
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += ViewModelsCollectionChanged;
            }

            InvalidateMeasure();
            InvalidateVisual();
        }

        void ViewModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    InvalidateMeasure();
                    break;
            }

            InvalidateVisual();
        }

        static void HandleViewModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ExpandoLabelsView)?.ViewModelsChanged(e);
        }
    }
}
