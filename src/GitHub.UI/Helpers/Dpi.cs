using System;
using System.Windows;

namespace GitHub.UI
{
    public struct Dpi
    {
        const double defaultDpi = 96.0;
        readonly double horizontal;
        readonly double horizontalScale;
        readonly double vertical;
        readonly double verticalScale;

        public static readonly Dpi Default = new Dpi(defaultDpi, defaultDpi);

        public Dpi(double horizontal, double vertical)
        {
            this.horizontal = horizontal;
            horizontalScale = horizontal / defaultDpi;
            this.vertical = vertical;
            verticalScale = vertical / defaultDpi;
        }

        public double Horizontal { get { return horizontal; } }
        public double HorizontalScale { get { return horizontalScale; } }
        public double Vertical { get { return vertical; } }
        public double VerticalScale { get { return verticalScale; } }

        public Point Scale(Point point)
        {
            return new Point(point.X * HorizontalScale, point.Y * VerticalScale);
        }

        public override bool Equals(object obj)
        {
            return obj is Dpi && this == (Dpi)obj;
        }

        public override int GetHashCode()
        {
            // Implementation from Jon Skeet: http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ Horizontal.GetHashCode();
                hash = hash * 16777619 ^ Vertical.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Dpi x, Dpi y)
        {
            const double epsilon = 0.001;

            // Since we're comparing double's we need to use an epsilon because LOL floating point numbers.
            return Math.Abs(x.Horizontal - y.Horizontal) < epsilon
                && Math.Abs(x.Vertical - y.Vertical) < epsilon;
        }

        public static bool operator !=(Dpi x, Dpi y)
        {
            return !(x == y);
        }
    }
}
