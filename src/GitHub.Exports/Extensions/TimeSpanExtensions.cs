using System;
using System.Globalization;

namespace GitHub.UI.Converters
{
    public static class TimeSpanExtensions
    {
        public static string Humanize(TimeSpan duration, CultureInfo culture, OutputTense outputTense = OutputTense.Past)
        {
            if (duration.Ticks <= 0)
            {
                return Resources.JustNow;
            }

            const int year = 365;
            const int month = 30;
            const int day = 24;
            const int hour = 60;
            const int minute = 60;

            if (duration.TotalDays >= year)
            {
                return GetFormattedValue(culture, (int) (duration.TotalDays / year), 
                    outputTense,
                    Resources.YearsAgo, Resources.Years,
                    Resources.YearAgo, Resources.Year);
            }

            if (duration.TotalDays >= 360)
            {
                return string.Format(culture, outputTense == OutputTense.Past ? Resources.MonthsAgo : Resources.Month, 11);
            }

            if (duration.TotalDays >= month)
            {
                return GetFormattedValue(culture, (int)(duration.TotalDays / month),
                    outputTense,
                    Resources.MonthsAgo, Resources.Months,
                    Resources.MonthAgo, Resources.Month);
            }

            if (duration.TotalHours >= day)
            {
                return GetFormattedValue(culture, (int)(duration.TotalHours / day),
                    outputTense,
                    Resources.DaysAgo, Resources.Days,
                    Resources.DayAgo, Resources.Day);
            }

            if (duration.TotalMinutes >= hour)
            {
                return GetFormattedValue(culture, (int)(duration.TotalMinutes / hour),
                    outputTense,
                    Resources.HoursAgo, Resources.Hours,
                    Resources.HourAgo, Resources.Hour);
            }

            if (duration.TotalSeconds >= minute)
            {
                return GetFormattedValue(culture, (int)(duration.TotalSeconds / minute),
                    outputTense,
                    Resources.MinutesAgo, Resources.Minutes,
                    Resources.MinuteAgo, Resources.Minute);
            }

            return GetFormattedValue(culture, (int) duration.TotalSeconds,
                outputTense,
                Resources.SecondsAgo, Resources.Seconds,
                Resources.SecondAgo, Resources.Second);
        }

        private static string GetFormattedValue(CultureInfo culture, int value, OutputTense outputTense,
            string multiplePast, string multipleCompleted,
            string singlePast, string singleCompleted)
        {
            var formatString = value > 1
                ? outputTense == OutputTense.Past ? multiplePast : multipleCompleted
                : outputTense == OutputTense.Past ? singlePast : singleCompleted;

            return string.Format(culture, formatString, value);
        }

        public enum OutputTense
        {
            Past,
            Completed
        }
    }
}