using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Extensions
{
    public static class DateTimeExtensions
    {
    }

    public static class DateTimeOffsetExtensions
    {
        // http://blogs.msdn.com/b/shawnste/archive/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net.aspx
        public static int GetIso8601WeekOfYear(this DateTimeOffset time)
        {
            Calendar cal = CultureInfo.InvariantCulture.Calendar;

            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time.UtcDateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time.UtcDateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
