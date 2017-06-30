using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using NullGuard;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class MultiBooleanToVisibilityConverter : MultiValueConverterMarkupExtension<MultiBooleanToVisibilityConverter>
    {
        readonly System.Windows.Controls.BooleanToVisibilityConverter converter = new System.Windows.Controls.BooleanToVisibilityConverter();

        public override object Convert(
            [AllowNull]object[] value,
            [AllowNull]Type targetType,
            [AllowNull]object parameter,
            [AllowNull]CultureInfo culture)
        {
            return value.OfType<bool>().All(x => x) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object[] ConvertBack(
            [AllowNull]object value,
            [AllowNull]Type[] targetType,
            [AllowNull]object parameter,
            [AllowNull]CultureInfo culture)
        {
            return null;
        }
    }
}
