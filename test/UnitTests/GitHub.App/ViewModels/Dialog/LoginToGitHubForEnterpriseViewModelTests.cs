using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Services;
using GitHub.ViewModels.Dialog;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Octokit;
using Xunit;

public class LoginToGitHubForEnterpriseViewModelTests
{
    public class TheProbeStatusProperty : TestBaseClass
    {
        [Fact]
        public void InvalidUrlReturnsNone()
        {
            var scheduler = new TestScheduler();
            var target = CreateTarget(scheduler);

            target.EnterpriseUrl = "badurl";

            Assert.Equal(EnterpriseProbeStatus.None, target.ProbeStatus);
        }

        [Fact]
        public void ReturnsCheckingWhenProbeNotFinished()
        {
            var scheduler = new TestScheduler();
            var caps = Substitute.For<IEnterpriseCapabilitiesService>();
            var task = new TaskCompletionSource<EnterpriseProbeResult>();
            caps.Probe(null).ReturnsForAnyArgs(task.Task);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.Equal(EnterpriseProbeStatus.Checking, target.ProbeStatus);
            task.SetCanceled();
        }

        [Fact]
        public void ReturnsValidWhenProbeReturnsOk()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.Ok);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();

            Assert.Equal(EnterpriseProbeStatus.Valid, target.ProbeStatus);
        }

        [Fact]
        public void ReturnsInvalidWhenProbeReturnsFailed()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.Failed);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();

            Assert.Equal(EnterpriseProbeStatus.Invalid, target.ProbeStatus);
        }

        [Fact]
        public void ReturnsInvalidWhenProbeReturnsNotFound()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.NotFound);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();

            Assert.Equal(EnterpriseProbeStatus.Invalid, target.ProbeStatus);
        }
    }

    public class TheSupportedLoginMethodsProperty : TestBaseClass
    {
        [Fact]
        public void InvalidUrlReturnsNull()
        {
            var scheduler = new TestScheduler();
            var target = CreateTarget(scheduler);

            target.EnterpriseUrl = "badurl";

            Assert.Null(target.SupportedLoginMethods);
        }

        [Fact]
        public void ReturnsToken()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.Equal(EnterpriseLoginMethods.Token, target.SupportedLoginMethods);
        }

        [Fact]
        public void ReturnsUsernameAndPassword()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.Equal(EnterpriseLoginMethods.UsernameAndPassword, target.SupportedLoginMethods);
        }

        [Fact]
        public void GivesPrecedenceToUsernameAndPasswordOverToken()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token | 
                EnterpriseLoginMethods.UsernameAndPassword |
                EnterpriseLoginMethods.OAuth);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.Equal(
                EnterpriseLoginMethods.UsernameAndPassword | EnterpriseLoginMethods.OAuth,
                target.SupportedLoginMethods);
        }
    }

    public class TheLoginCommand : TestBaseClass
    {
        [Fact]
        public void DisabledWhenUserNameEmpty()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.Password = "pass";

            Assert.False(target.Login.CanExecute(null));
        }

        [Fact]
        public void DisabledWhenPasswordEmpty()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.UsernameOrEmail = "user";

            Assert.False(target.Login.CanExecute(null));
        }

        [Fact]
        public void EnabledWhenUsernameAndPasswordSet()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.UsernameOrEmail = "user";
            target.Password = "pass";

            Assert.True(target.Login.CanExecute(null));
        }

        [Fact]
        public void EnabledWhenOnlyPasswordSetWhenUsingTokenLogin()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.Password = "pass";

            Assert.True(target.Login.CanExecute(null));
        }
    }

    static IEnterpriseCapabilitiesService CreateCapabilties(EnterpriseProbeResult probeResult)
    {
        var result = Substitute.For<IEnterpriseCapabilitiesService>();
        result.Probe(null).ReturnsForAnyArgs(probeResult);
        return result;
    }

    static IEnterpriseCapabilitiesService CreateCapabilties(EnterpriseLoginMethods methods)
    {
        var result = CreateCapabilties(EnterpriseProbeResult.Ok);
        result.ProbeLoginMethods(null).ReturnsForAnyArgs(methods);
        return result;
    }

    static LoginToGitHubForEnterpriseViewModel CreateTarget(
        IScheduler scheduler = null,
        IEnterpriseCapabilitiesService capabilitiesService = null)
    {
        return new LoginToGitHubForEnterpriseViewModel(
            Substitute.For<IConnectionManager>(),
            capabilitiesService ?? Substitute.For<IEnterpriseCapabilitiesService>(),
            Substitute.For<IVisualStudioBrowser>(),
            scheduler ?? Scheduler.Immediate);
    }
}
