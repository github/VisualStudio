using System;
using System.Globalization;

namespace GitHub.UI
{
    public class PositiveDurationToStringConverter : ValueConverterMarkupExtension<PositiveDurationToStringConverter>
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

            if (duration.Ticks <= 0)
            {
                return Resources.JustNow;
            }

            return DurationToStringConverter.FormatDuration(duration, culture);
        }
    }
}