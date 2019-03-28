using System;
using System.Globalization;
using GitHub.UI.Converters;

namespace GitHub.UI
{
    public class DurationToStringConverter : ValueConverterMarkupExtension<DurationToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan duration;
            if (value is TimeSpan span)
                duration = span;
            else if (value is DateTime time)
                duration = DateTime.UtcNow - time;
            else if (value is DateTimeOffset offset)
                duration = DateTimeOffset.UtcNow - offset;
            else
                return value;

            return TimeSpanExtensions.Humanize(duration, culture);
        }
    }
}