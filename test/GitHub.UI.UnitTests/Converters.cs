using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.UI;
using NUnit.Framework;
using System.Globalization;

public class Converters
{
    [TestCase(0, 0, -23, 0, "just now")]
    [TestCase(-2, 0, 0, 0, "just now")]
    [TestCase(-1, 0, 0, 0, "just now")]
    [TestCase(0, 0, 0, 0, "just now")]
    [TestCase(1, 0, 0, 0, "1 second ago")]
    [TestCase(2, 0, 0, 0, "2 seconds ago")]
    [TestCase(59, 0, 0, 0, "59 seconds ago")]
    [TestCase(0, 1, 0, 0, "1 minute ago")]
    [TestCase(0, 2, 0, 0, "2 minutes ago")]
    [TestCase(0, 59, 0, 0, "59 minutes ago")]
    [TestCase(0, 60, 0, 0, "1 hour ago")]
    [TestCase(0, 0, 1, 0, "1 hour ago")]
    [TestCase(0, 0, 2, 0, "2 hours ago")]
    [TestCase(0, 0, 23, 0, "23 hours ago")]
    [TestCase(0, 0, 24, 0, "1 day ago")]
    [TestCase(0, 0, 0, 1, "1 day ago")]
    [TestCase(0, 0, 0, 2, "2 days ago")]
    [TestCase(0, 0, 0, 29, "29 days ago")]
    [TestCase(0, 0, 0, 30, "1 month ago")]
    [TestCase(0, 0, 0, 59, "1 month ago")]
    [TestCase(0, 0, 0, 60, "2 months ago")]
    [TestCase(0, 0, 0, 364, "11 months ago")]
    [TestCase(0, 0, 0, 365, "1 year ago")]
    [TestCase(0, 0, 0, 365*2-1, "1 year ago")]
    [TestCase(0, 0, 0, 365*2, "2 years ago")]
    public void DurationToStringConversion(int sec, int min, int hou, int day, string expected)
    {
        var ts = new TimeSpan(day, hou, min, sec);
        var conv = new DurationToStringConverter();
        var ret = (string)conv.Convert(ts, typeof(string), null, CultureInfo.CurrentCulture);
        Assert.That(ret, Is.EqualTo(expected));
    }
}
