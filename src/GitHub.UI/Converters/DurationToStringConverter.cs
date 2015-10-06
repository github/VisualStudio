using System;
using System.Globalization;

namespace GitHub.UI
{
    public class DurationToStringConverter : ValueConverterMarkupExtension<DurationToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int year = 356;
            const int month = 30;
            const int day = 24;
            const int hour = 60;
            const int minute = 60;

            TimeSpan duration;
            if (value is TimeSpan)
                duration = (TimeSpan)value;
            else if (value is DateTime)
                duration = DateTime.Now - (DateTime)value;
            else if (value is DateTimeOffset)
                duration = DateTimeOffset.Now - (DateTimeOffset)value;
            else
                return value;

            if (duration.TotalDays >= year)
                return string.Format(CultureInfo.CurrentCulture, (int)(duration.TotalDays / year) > 1 ? Resources.years : Resources.year, duration.TotalDays / year);
            else if (duration.TotalDays >= month)
                return string.Format(CultureInfo.CurrentCulture, (int)(duration.TotalDays / month) > 1 ? Resources.months : Resources.year, duration.TotalDays / month);
            else if (duration.TotalHours >= day)
                return string.Format(CultureInfo.CurrentCulture, (int)(duration.TotalHours / day) > 1 ? Resources.days : Resources.day, duration.TotalHours / day);
            else if (duration.TotalMinutes >= hour)
                return string.Format(CultureInfo.CurrentCulture, (int)(duration.TotalMinutes / hour) > 1 ? Resources.hours : Resources.hour, duration.TotalMinutes / hour);
            else if (duration.TotalSeconds >= minute)
                return string.Format(CultureInfo.CurrentCulture, (int)(duration.TotalSeconds / minute) > 1 ? Resources.minutes : Resources.minute, duration.TotalSeconds / minute);
            return string.Format(CultureInfo.CurrentCulture, duration.TotalSeconds > 1 ? Resources.seconds : Resources.second, duration.TotalSeconds);
        }
    }
}
