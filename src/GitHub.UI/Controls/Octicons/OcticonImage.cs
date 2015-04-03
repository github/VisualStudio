using System.Windows;
using System.Windows.Controls;
using NullGuard;

namespace GitHub.UI
{
    public class OcticonImage : Control
    {
        public Octicon Icon
        {
            [return: AllowNull]
            get { return (Octicon)GetValue(OcticonPath.IconProperty); }
            set { SetValue(OcticonPath.IconProperty, value); }
        }

        public static DependencyProperty IconProperty =
            OcticonPath.IconProperty.AddOwner(typeof(OcticonImage));
    }
}
