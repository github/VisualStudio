using System;
using System.Globalization;
using System.Windows;
using NullGuard;

namespace GitHub.UI
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class BooleanToHiddenVisibilityConverter : ValueConverterMarkupExtension<BooleanToHiddenVisibilityConverter>
    {
        public override object Convert(
            object value,
            [AllowNull]Type targetType,
            [AllowNull]object parameter,
            [AllowNull]CultureInfo culture)
        {
            return value is bool && (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value,
            [AllowNull]Type targetType,
            [AllowNull]object parameter,
            [AllowNull]CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
