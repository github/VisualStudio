using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Controls
{
    /// <summary>
    /// Interaction logic for PullRequestStatusCircle.xaml
    /// </summary>
    public partial class PullRequestStatusCircle : UserControl
    {
        public static readonly DependencyProperty ErrorCountProperty = DependencyProperty.Register(
            "ErrorCount", typeof(int), typeof(PullRequestStatusCircle),
            new PropertyMetadata(0, GeneratePolygons));

        public static readonly DependencyProperty SuccessCountProperty = DependencyProperty.Register(
            "SuccessCount", typeof(int), typeof(PullRequestStatusCircle),
            new PropertyMetadata(0, GeneratePolygons));

        public static readonly DependencyProperty PendingCountProperty = DependencyProperty.Register(
            "PendingCount", typeof(int), typeof(PullRequestStatusCircle),
            new PropertyMetadata(0, GeneratePolygons));

        private static void GeneratePolygons(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pullRequestStatusCircle = ((PullRequestStatusCircle)dependencyObject);
            pullRequestStatusCircle.GeneratePolygons();
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(PullRequestStatusCircle),
            new PropertyMetadata((double)250, GenerateMaskAndPolygons));

        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register(
            "InnerRadius", typeof(double), typeof(PullRequestStatusCircle),
            new PropertyMetadata((double)200, GenerateMaskAndPolygons));

        private static void GenerateMaskAndPolygons(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pullRequestStatusCircle = ((PullRequestStatusCircle) dependencyObject);
            pullRequestStatusCircle.GenerateMask();
            pullRequestStatusCircle.GeneratePolygons();
        }

        public static readonly DependencyProperty PendingColorProperty = DependencyProperty.Register(
            "PendingColor", typeof(Brush), typeof(PullRequestStatusCircle),
            new PropertyMetadata(Brushes.Yellow, OnPendingColorChanged));

        private static void OnPendingColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pullRequestStatusCircle = ((PullRequestStatusCircle) dependencyObject);
            pullRequestStatusCircle.PendingPolygon.Fill = (Brush) eventArgs.NewValue;
        }

        public static readonly DependencyProperty ErrorColorProperty = DependencyProperty.Register(
            "ErrorColor", typeof(Brush), typeof(PullRequestStatusCircle),
            new PropertyMetadata(Brushes.Red, OnErrorColorChanged));

        private static void OnErrorColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pullRequestStatusCircle = ((PullRequestStatusCircle) dependencyObject);
            pullRequestStatusCircle.ErrorPolygon.Fill = (Brush) eventArgs.NewValue;
        }

        public static readonly DependencyProperty SuccessColorProperty = DependencyProperty.Register(
            "SuccessColor", typeof(Brush), typeof(PullRequestStatusCircle),
            new PropertyMetadata(Brushes.Green, OnSuccessColorChanged));

        private static void OnSuccessColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pullRequestStatusCircle = ((PullRequestStatusCircle) dependencyObject);
            pullRequestStatusCircle.SuccessPolygon.Fill = (Brush) eventArgs.NewValue;
        }

        public IEnumerable<Point> GeneratePoints(float percentage)
        {
            double ToRadians(float val)
            {
                return (Math.PI / 180) * val;
            }

            if (float.IsNaN(percentage))
            {
                return Array.Empty<Point>();
            }

            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentException();
            }

            var diameter = Diameter;

            var leftEdge = XAdjust;
            var rightEdge = diameter + XAdjust;
            var topEdge = YAdjust;
            var bottomEdge = diameter + YAdjust;

            var topMiddle = new Point(Origin.X, topEdge);
            var topRight = new Point(rightEdge, topEdge);
            var bottomRight = new Point(rightEdge, bottomEdge);
            var bottomLeft = new Point(leftEdge, bottomEdge);
            var topLeft = new Point(leftEdge, topEdge);

            if (percentage == 1)
            {
                return new[] { topLeft, topRight, bottomRight, bottomLeft };
            }

            var degrees = percentage * 360;
            var adjustedDegrees = (degrees + 90) % 360;

            if (adjustedDegrees >= 90 && adjustedDegrees < 135)
            {
                var angleDegrees = adjustedDegrees - 90;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, new Point(topMiddle.X + oppositeEdge, topMiddle.Y) };
            }

            if (adjustedDegrees >= 135 && adjustedDegrees < 180)
            {
                var angleDegrees = adjustedDegrees - 135;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, new Point(topRight.X, topRight.Y + oppositeEdge) };
            }

            if (adjustedDegrees >= 180 && adjustedDegrees < 225)
            {
                var angleDegrees = adjustedDegrees - 180;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, new Point(topRight.X, topRight.Y + Radius + oppositeEdge) };
            }

            if (adjustedDegrees >= 225 && adjustedDegrees < 270)
            {
                var angleDegrees = adjustedDegrees - 225;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, bottomRight, new Point(bottomRight.X - oppositeEdge, bottomRight.Y) };
            }

            if (adjustedDegrees >= 270 && adjustedDegrees < 315)
            {
                var angleDegrees = adjustedDegrees - 270;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, bottomRight, new Point(bottomRight.X - Radius - oppositeEdge, bottomRight.Y) };
            }

            if (adjustedDegrees >= 315 && adjustedDegrees < 360)
            {
                var angleDegrees = adjustedDegrees - 315;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, bottomRight, bottomLeft, new Point(bottomLeft.X, bottomLeft.Y - oppositeEdge) };
            }

            if (adjustedDegrees >= 0 && adjustedDegrees < 45)
            {
                var angleDegrees = adjustedDegrees;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, bottomRight, bottomLeft, new Point(bottomLeft.X, bottomLeft.Y - Radius - oppositeEdge) };
            }

            if (adjustedDegrees >= 45 && adjustedDegrees < 90)
            {
                var angleDegrees = adjustedDegrees - 45;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * Radius;
                return new[] { Origin, topMiddle, topRight, bottomRight, bottomLeft, topLeft, new Point(topLeft.X + oppositeEdge, topLeft.Y) };
            }

            throw new InvalidOperationException();
        }

        public PullRequestStatusCircle()
        {
            InitializeComponent();
            GeneratePolygons();
            GenerateMask();
        }

        private void GeneratePolygons()
        {
            ErrorPolygon.Points = new PointCollection(GeneratePoints((float)ErrorCount / TotalCount));
            SuccessPolygon.Points = new PointCollection(GeneratePoints((float)(SuccessCount + ErrorCount) / TotalCount));
            PendingPolygon.Points = new PointCollection(GeneratePoints((float)(SuccessCount + ErrorCount + PendingCount) / TotalCount));
        }

        private void GenerateMask()
        {
            var pendingPolygonClip = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                new EllipseGeometry(Origin, Radius, Radius),
                new EllipseGeometry(Origin, InnerRadius, InnerRadius));

            PendingPolygon.Clip = pendingPolygonClip;
            SuccessPolygon.Clip = pendingPolygonClip;
            ErrorPolygon.Clip = pendingPolygonClip;
        }

        private Point Origin => new Point(Radius + XAdjust, Radius + YAdjust);

        private double Diameter => Radius * 2;

        private double XAdjust => (ActualWidth - Diameter) / 2;

        private double YAdjust => (ActualHeight - Diameter) / 2;

        private int TotalCount => ErrorCount + SuccessCount + PendingCount;

        public int ErrorCount
        {
            get => (int)GetValue(ErrorCountProperty);
            set
            {
                SetValue(ErrorCountProperty, value);
            }
        }

        public int SuccessCount
        {
            get => (int)GetValue(SuccessCountProperty);
            set
            {
                SetValue(SuccessCountProperty, value);
            }
        }

        public int PendingCount
        {
            get => (int)GetValue(PendingCountProperty);
            set
            {
                SetValue(PendingCountProperty, value);
            }
        }

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set
            {
                SetValue(RadiusProperty, value);
            }
        }

        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set
            {
                SetValue(InnerRadiusProperty, value);
            }
        }

        public Brush PendingColor
        {
            get => (Brush)GetValue(PendingColorProperty);
            set
            {
                SetValue(PendingColorProperty, value);
            }
        }

        public Brush ErrorColor
        {
            get => (Brush)GetValue(ErrorColorProperty);
            set
            {
                SetValue(ErrorColorProperty, value);
            }
        }

        public Brush SuccessColor
        {
            get => (Brush)GetValue(SuccessColorProperty);
            set
            {
                SetValue(SuccessColorProperty, value);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.WidthChanged || sizeInfo.HeightChanged)
            {
                GenerateMask();
                GeneratePolygons();
            }
        }
    }
}
