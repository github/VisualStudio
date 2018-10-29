using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Services;
using GitHub.ViewModels.Dialog;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Octokit;
using NUnit.Framework;
using System.Windows.Input;
using ReactiveUI.Testing;
using ReactiveUI;

public class LoginToGitHubForEnterpriseViewModelTests
{
    public class TheProbeStatusProperty : TestBaseClass
    {
        [Test]
        public void InvalidUrlReturnsNone()
        {
            var scheduler = new TestScheduler();
            var target = CreateTarget(scheduler);

            target.EnterpriseUrl = "badurl";

            Assert.That(EnterpriseProbeStatus.None, Is.EqualTo(target.ProbeStatus));
        }

        [Test]
        public async Task ReturnsCheckingWhenProbeNotFinished()
        {
            Console.WriteLine(RxApp.MainThreadScheduler.GetType().Name);

            var scheduler = new TestScheduler();
            var caps = Substitute.For<IEnterpriseCapabilitiesService>();
            var task = new TaskCompletionSource<EnterpriseProbeResult>();
            caps.Probe(null).ReturnsForAnyArgs(task.Task);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.That(EnterpriseProbeStatus.Checking, Is.EqualTo(target.ProbeStatus));

            try
            {
                task.SetCanceled();
                await task.Task;
            }
            catch (TaskCanceledException) { }
        }

        [Test]
        public async Task ReturnsValidWhenProbeReturnsOk()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.Ok);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();
            await target.UpdatingProbeStatus;

            Assert.That(EnterpriseProbeStatus.Valid, Is.EqualTo(target.ProbeStatus));
        }

        [Test]
        public async Task ReturnsInvalidWhenProbeReturnsFailed()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.Failed);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();
            await target.UpdatingProbeStatus;

            Assert.That(EnterpriseProbeStatus.Invalid, Is.EqualTo(target.ProbeStatus));
        }

        [Test]
        public async Task ReturnsInvalidWhenProbeReturnsNotFound()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseProbeResult.NotFound);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            scheduler.Stop();
            await target.UpdatingProbeStatus;

            Assert.That(EnterpriseProbeStatus.Invalid, Is.EqualTo(target.ProbeStatus));
        }
    }

    public class TheSupportedLoginMethodsProperty : TestBaseClass
    {
        [Test]
        public void InvalidUrlReturnsNull()
        {
            var scheduler = new TestScheduler();
            var target = CreateTarget(scheduler);

            target.EnterpriseUrl = "badurl";

            Assert.That(target.SupportedLoginMethods, Is.Null);
        }

        [Test]
        public void ReturnsToken()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.That(EnterpriseLoginMethods.Token, Is.EqualTo(target.SupportedLoginMethods));
        }

        [Test]
        public void ReturnsUsernameAndPassword()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.That(EnterpriseLoginMethods.UsernameAndPassword, Is.EqualTo(target.SupportedLoginMethods));
        }

        [Test]
        public void GivesPrecedenceToUsernameAndPasswordOverToken()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token |
                EnterpriseLoginMethods.UsernameAndPassword |
                EnterpriseLoginMethods.OAuth);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

            Assert.That(
                EnterpriseLoginMethods.UsernameAndPassword | EnterpriseLoginMethods.OAuth,
                Is.EqualTo(target.SupportedLoginMethods));
        }
    }

    public class TheLoginCommand : TestBaseClass
    {
        [Test]
        public void DisabledWhenUserNameEmpty()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.Password = "pass";

            Assert.False(((ICommand)target.Login).CanExecute(null));
        }

        [Test]
        public void DisabledWhenPasswordEmpty()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.UsernameOrEmail = "user";

            Assert.False(((ICommand)target.Login).CanExecute(null));
        }

        [Test]
        public void EnabledWhenUsernameAndPasswordSet()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.UsernameAndPassword);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.UsernameOrEmail = "user";
            target.Password = "pass";

            Assert.True(((ICommand)target.Login).CanExecute(null));
        }

        [Test]
        public void EnabledWhenOnlyPasswordSetWhenUsingTokenLogin()
        {
            var scheduler = new TestScheduler();
            var caps = CreateCapabilties(EnterpriseLoginMethods.Token);
            var target = CreateTarget(scheduler, caps);

            target.EnterpriseUrl = "https://foo.bar";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
            target.Password = "pass";

            Assert.True(((ICommand)target.Login).CanExecute(null));
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
