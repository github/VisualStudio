using System;
using System.Collections;
using System.Reactive.Subjects;
using System.Text;
using Xunit.Abstractions;

public class TestBase
{
    protected readonly ITestOutputHelper output;
    protected StringBuilder testOutput = new StringBuilder();

#if DEBUG
    public TestBase(ITestOutputHelper output)
    {
        this.output = output;
    }
#endif

    protected void Dump(string msg)
    {
        output?.WriteLine(msg);
        testOutput.AppendLine(msg);
    }

    protected void Dump(object prefix, object thing)
    {
        output?.WriteLine($"{prefix} - {thing}");
        testOutput.AppendLine($"{prefix} - {thing}");
    }

    protected void Dump(object thing)
    {
        output?.WriteLine(thing.ToString());
        testOutput.AppendLine(thing.ToString());
    }
    protected void Dump(string title, IEnumerable col)
    {
        output?.WriteLine(title);
        testOutput.AppendLine(title);
        var i = 0;
        foreach (var l in col)
            Dump(i++, l);
    }

    protected void Dump(IEnumerable col)
    {
        Dump("Dumping", col);
    }

    protected bool Compare(Thing thing1, Thing thing2)
    {
        return Equals(thing1, thing2) && thing1.Title == thing2.Title && thing1.CreatedAt == thing2.CreatedAt && thing1.UpdatedAt == thing2.UpdatedAt;
    }

    protected void Add(Subject<Thing> source, Thing item)
    {
        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        source.OnNext(item);
    }

    protected Thing GetThing(int id)
    {
        return new Thing
        {
            Number = id
        };
    }

    protected Thing GetThing(int id, int minutes)
    {
        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        return new Thing
        {
            Number = id,
            Title = "Run 1",
            CreatedAt = now + TimeSpan.FromMinutes(minutes),
            UpdatedAt = now + TimeSpan.FromMinutes(minutes)
        };
    }

    protected Thing GetThing(int id, int minutesc, int minutesu)
    {
        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        return new Thing
        {
            Number = id,
            Title = "Run 1",
            CreatedAt = now + TimeSpan.FromMinutes(minutesc),
            UpdatedAt = now + TimeSpan.FromMinutes(minutesu)
        };
    }

    protected Thing GetThing(int id, string title)
    {
        return new Thing
        {
            Number = id,
            Title = title,
        };
    }

    protected Thing GetThing(int id, int minutesc, int minutesu, string title)
    {
        var now = new DateTimeOffset(0, TimeSpan.FromTicks(0));
        return new Thing
        {
            Number = id,
            Title = title,
            CreatedAt = now + TimeSpan.FromMinutes(minutesc),
            UpdatedAt = now + TimeSpan.FromMinutes(minutesu)
        };
    }
}