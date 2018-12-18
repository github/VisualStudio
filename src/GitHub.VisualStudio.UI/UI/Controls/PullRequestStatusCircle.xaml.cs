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

namespace GitHub.VisualStudio.UI.UI.Controls
{
    /// <summary>
    /// Interaction logic for PullRequestStatusCircle.xaml
    /// </summary>
    public partial class PullRequestStatusCircle : UserControl
    {
        public static readonly DependencyProperty ErrorCountProperty = DependencyProperty.Register(
            "ErrorCount", typeof(int), typeof(PullRequestStatusCircle), new PropertyMetadata(0));

        public static readonly DependencyProperty SuccessCountProperty = DependencyProperty.Register(
            "SuccessCount", typeof(int), typeof(PullRequestStatusCircle), new PropertyMetadata(0));

        public static readonly DependencyProperty PendingCountProperty = DependencyProperty.Register(
            "PendingCount", typeof(int), typeof(PullRequestStatusCircle), new PropertyMetadata(0));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(PullRequestStatusCircle), new PropertyMetadata((double)250));

        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register(
            "InnerRadius", typeof(double), typeof(PullRequestStatusCircle), new PropertyMetadata((double)200));

        public static IEnumerable<Point> GeneratePoints(double size, float percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentException();
            }

            var halfSize = size / 2;
            var origin = new Point(halfSize, halfSize);
            var topMiddle = new Point(halfSize, 0);
            var topRight = new Point(size, 0);
            var bottomRight = new Point(size, size);
            var bottomLeft = new Point(0, size);
            var topLeft = new Point(0, 0);

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
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, new Point(halfSize + oppositeEdge, 0) };
            }

            if (adjustedDegrees >= 135 && adjustedDegrees < 180)
            {
                var angleDegrees = adjustedDegrees - 135;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, new Point(size, oppositeEdge) };
            }

            if (adjustedDegrees >= 180 && adjustedDegrees < 225)
            {
                var angleDegrees = adjustedDegrees - 180;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, new Point(size, halfSize + oppositeEdge) };
            }

            if (adjustedDegrees >= 225 && adjustedDegrees < 270)
            {
                var angleDegrees = adjustedDegrees - 225;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, bottomRight, new Point(size - oppositeEdge, size) };
            }

            if (adjustedDegrees >= 270 && adjustedDegrees < 315)
            {
                var angleDegrees = adjustedDegrees - 270;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, bottomRight, new Point(halfSize - oppositeEdge, size) };
            }

            if (adjustedDegrees >= 315 && adjustedDegrees < 360)
            {
                var angleDegrees = adjustedDegrees - 315;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, bottomRight, bottomLeft, new Point(0, size - oppositeEdge) };
            }

            if (adjustedDegrees >= 0 && adjustedDegrees < 45)
            {
                var angleDegrees = adjustedDegrees;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, bottomRight, bottomLeft, new Point(0, halfSize - oppositeEdge) };
            }

            if (adjustedDegrees >= 45 && adjustedDegrees < 90)
            {
                var angleDegrees = adjustedDegrees - 45;
                var angleRadians = ToRadians(angleDegrees);
                var tan = Math.Tan(angleRadians);
                var oppositeEdge = tan * halfSize;
                return new[] { origin, topMiddle, topRight, bottomRight, bottomLeft, topLeft, new Point(oppositeEdge, 0) };
            }

            return new Point[0];
        }

        public static double ToRadians(float val)
        {
            return (Math.PI / 180) * val;
        }

        public PullRequestStatusCircle()
        {
            InitializeComponent();
            GeneratePolygons();
            ComputeMask();
        }

        private void GeneratePolygons()
        {
            ErrorPolygon.Points = new PointCollection(GeneratePoints(Radius * 2, (float)ErrorCount / TotalCount));
            SuccessPolygon.Points = new PointCollection(GeneratePoints(Radius * 2, (float)(SuccessCount + ErrorCount) / TotalCount));
            PendingPolygon.Points = new PointCollection(GeneratePoints(Radius * 2, (float)(SuccessCount + ErrorCount + PendingCount) / TotalCount));
        }

        private void ComputeMask()
        {
            var pendingPolygonClip = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                new EllipseGeometry(Center, Radius, Radius),
                new EllipseGeometry(Center, InnerRadius, InnerRadius));

            PendingPolygon.Clip = pendingPolygonClip;
            SuccessPolygon.Clip = pendingPolygonClip;
            ErrorPolygon.Clip = pendingPolygonClip;
        }

        private int TotalCount => ErrorCount + SuccessCount + PendingCount;

        private Point Center => new Point(Radius, Radius);

        public int ErrorCount
        {
            get => (int)GetValue(ErrorCountProperty);
            set
            {
                SetValue(ErrorCountProperty, value);
                ComputeMask();
                GeneratePolygons();
            }
        }

        public int SuccessCount
        {
            get => (int)GetValue(SuccessCountProperty);
            set
            {
                SetValue(SuccessCountProperty, value);
                ComputeMask();
                GeneratePolygons();
            }
        }

        public int PendingCount
        {
            get => (int)GetValue(PendingCountProperty);
            set
            {
                SetValue(PendingCountProperty, value);
                ComputeMask();
                GeneratePolygons();
            }
        }

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set
            {
                SetValue(RadiusProperty, value);
                ComputeMask();
                GeneratePolygons();
            }
        }

        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set
            {
                SetValue(InnerRadiusProperty, value);
                ComputeMask();
                GeneratePolygons();
            }
        }
    }
}
