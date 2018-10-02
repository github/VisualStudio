using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Authentication;
using GitHub.Services;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using Octokit;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.Dialog
{
    public class Login2FaViewModelTests
    {
        public class TheShowMethod
        {
            [Test]
            public void ClearsIsBusy()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();

                target.OkCommand.Execute();
                target.Show(new TwoFactorRequiredUserError(exception));

                Assert.False(target.IsBusy);
            }

            [Test]
            public void InvalidAuthenticationCodeIsSetWhenRetryFailed()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();

                target.Show(new TwoFactorRequiredUserError(exception));

                Assert.True(target.InvalidAuthenticationCode);
            }

            [Test]
            public async Task OkCommandCompletesAndReturnsNullWithNoAuthorizationCodeAsync()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();
                var userError = new TwoFactorRequiredUserError(exception);
                var task = target.Show(userError).ToTask();

                target.OkCommand.Execute().Subscribe();
                var result = await task;

                Assert.That(result, Is.Null);
            }

            [Test]
            public async Task OkCommandCompletesAndReturnsAuthorizationCodeAsync()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();
                var userError = new TwoFactorRequiredUserError(exception);
                var task = target.Show(userError).ToTask();

                target.AuthenticationCode = "123456";
                target.OkCommand.Execute().Subscribe();

                var result = await task;
                Assert.That("123456", Is.EqualTo(result.AuthenticationCode));
            }

            [Test]
            public async Task ResendCodeCommandCompletesAndReturnsRequestResendCodeAsync()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();
                var userError = new TwoFactorRequiredUserError(exception);
                var task = target.Show(userError).ToTask();

                target.AuthenticationCode = "123456";
                target.ResendCodeCommand.Execute().Subscribe();
                var result = await task;

                Assert.False(target.IsBusy);
                Assert.That(TwoFactorChallengeResult.RequestResendCode, Is.EqualTo(result));
            }
           
            [Test]
            public async Task ShowErrorMessageIsClearedWhenAuthenticationCodeSentAsync()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();
                var userError = new TwoFactorRequiredUserError(exception);
                var task = target.Show(userError).ToTask();

                Assert.True(target.ShowErrorMessage);
                target.ResendCodeCommand.Execute().Subscribe();

                var result = await task;
                Assert.False(target.ShowErrorMessage);
            }
        }

        public class TheCancelMethod
        {
            [Test]
            public async Task CancelCommandCompletesAndReturnsNullAsync()
            {
                var target = CreateTarget();
                var exception = new TwoFactorChallengeFailedException();
                var userError = new TwoFactorRequiredUserError(exception, TwoFactorType.AuthenticatorApp);
                var task = target.Show(userError).ToTask();

                target.AuthenticationCode = "123456";
                target.Cancel();
                var result = await task;

                Assert.False(target.IsBusy);
                Assert.That(result, Is.Null);
            }

            [Test]
            public async Task Cancel_Resets_TwoFactorType_Async()
            {
                var target = CreateTarget();
                var exception = new TwoFactorRequiredException(TwoFactorType.Sms);
                var userError = new TwoFactorRequiredUserError(exception);
                var task = target.Show(userError).ToTask();

                Assert.That(TwoFactorType.Sms, Is.EqualTo(target.TwoFactorType));

                target.Cancel();
                await task;

                // TwoFactorType must be cleared here as the UIController uses it as a trigger
                // to show the 2FA dialog view.
                Assert.That(TwoFactorType.None, Is.EqualTo(target.TwoFactorType));
            }
        }

        static Login2FaViewModel CreateTarget()
        {
            var browser = Substitute.For<IVisualStudioBrowser>();
            var twoFactorChallengeHandler = Substitute.For<IDelegatingTwoFactorChallengeHandler>();
            return new Login2FaViewModel(browser);
        }
    }
}
