using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Octokit;
using UnitTests.Helpers;
using NUnit.Framework;
using UnitTests;

public class PullRequestModelTests : TestBaseClass
{
    protected static readonly DateTimeOffset Now = new DateTimeOffset(0, TimeSpan.FromTicks(0));

    [Test]
    public void ComparisonNullEqualsNull()
    {
        PullRequestModel left = null;
        PullRequestModel right = null;
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.False(left > right);
        Assert.False(left < right);
    }

    [Test]
    public void ComparisonBiggerThanNull()
    {
        PullRequestModel left = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now);
        PullRequestModel right = null;
        Assert.False(left == right);
        Assert.True(left != right);
        Assert.True(left > right);
        Assert.False(left < right);
    }

    [Test]
    public void ComparisonNullLowerThan()
    {
        PullRequestModel left = null;
        PullRequestModel right = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now);
        Assert.False(left == right);
        Assert.True(left != right);
        Assert.False(left > right);
        Assert.True(left < right);
    }

    [Test]
    public void ComparisonLowerThan()
    {
        PullRequestModel left = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(1));
        PullRequestModel right = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(2));
        Assert.False(left == right);
        Assert.True(left != right);
        Assert.False(left > right);
        Assert.True(left < right);
    }

    [Test]
    public void ComparisonGreaterThan()
    {
        PullRequestModel left = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(3));
        PullRequestModel right = new PullRequestModel(0, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(2));
        Assert.False(left == right);
        Assert.True(left != right);
        Assert.True(left > right);
        Assert.False(left < right);
    }

    [Test]
    public void ComparisonEquals()
    {
        PullRequestModel left = new PullRequestModel(1, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(1));
        PullRequestModel right = new PullRequestModel(1, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(1));
        Assert.False(left == right);
        Assert.True(left != right);
        Assert.False(left > right);
        Assert.False(left < right);
    }

    [TestCase(1, 1, 1, 2)]
    [TestCase(1, 1, 2, 1)]
    public void ComparisonNotEquals(int id1, int ms1, int id2, int ms2)
    {
        PullRequestModel left = new PullRequestModel(id1, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(ms1));
        PullRequestModel right = new PullRequestModel(id2, "", Substitute.For<IAccount>(), Now, Now + TimeSpan.FromMilliseconds(ms2));
        Assert.False(left == right);
        Assert.True(left != right);
    }
}