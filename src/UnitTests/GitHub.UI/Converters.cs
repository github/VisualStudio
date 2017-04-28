using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.UI;
using Xunit;
using System.Globalization;

public class Converters
{
    [Theory]
    [InlineData(1, 0, 0, 0, "1 second ago")]
    [InlineData(2, 0, 0, 0, "2 seconds ago")]
    [InlineData(59, 0, 0, 0, "59 seconds ago")]
    [InlineData(0, 1, 0, 0, "1 minute ago")]
    [InlineData(0, 2, 0, 0, "2 minutes ago")]
    [InlineData(0, 59, 0, 0, "59 minutes ago")]
    [InlineData(0, 60, 0, 0, "1 hour ago")]
    [InlineData(0, 0, 1, 0, "1 hour ago")]
    [InlineData(0, 0, 2, 0, "2 hours ago")]
    [InlineData(0, 0, 23, 0, "23 hours ago")]
    [InlineData(0, 0, 24, 0, "1 day ago")]
    [InlineData(0, 0, 0, 1, "1 day ago")]
    [InlineData(0, 0, 0, 2, "2 days ago")]
    [InlineData(0, 0, 0, 29, "29 days ago")]
    [InlineData(0, 0, 0, 30, "1 month ago")]
    [InlineData(0, 0, 0, 59, "1 month ago")]
    [InlineData(0, 0, 0, 60, "2 months ago")]
    [InlineData(0, 0, 0, 364, "11 months ago")]
    [InlineData(0, 0, 0, 365, "1 year ago")]
    [InlineData(0, 0, 0, 365*2-1, "1 year ago")]
    [InlineData(0, 0, 0, 365*2, "2 years ago")]
    public void TimespanConversion(int sec, int min, int hou, int day, string expected)
    {
        var ts = new TimeSpan(day, hou, min, sec);
        var conv = new DurationToStringConverter();
        var ret = (string)conv.Convert(ts, typeof(string), null, CultureInfo.CurrentCulture);
        Assert.Equal(expected, ret);
    }
}
