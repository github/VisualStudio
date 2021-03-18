using System.Collections.Generic;
using System.Collections.Immutable;
using Octokit;
using NSubstitute;
using NUnit.Framework;
using GitHub.Extensions;

public class ApiExceptionExtensionsTests
{
    public class TheIsGitHubApiExceptionMethod
    {
        [TestCase("Not-GitHub-Request-Id", false)]
        [TestCase("X-GitHub-Request-Id", true)]
        [TestCase("x-github-request-id", true)]
        public void NoGitHubRequestId(string key, bool expect)
        {
            var ex = CreateApiException(new Dictionary<string, string> { { key, "ANYTHING" } });

            var result = ApiExceptionExtensions.IsGitHubApiException(ex);

            Assert.That(result, Is.EqualTo(expect));
        }
        
        [Test]
        public void NoResponse()
        {
            var ex = new ApiException();

            var result = ApiExceptionExtensions.IsGitHubApiException(ex);

            Assert.That(result, Is.EqualTo(false));
        }

        static ApiException CreateApiException(Dictionary<string, string> headers)
        {
            var response = Substitute.For<IResponse>();
            response.Headers.Returns(headers.ToImmutableDictionary());
            var ex = new ApiException(response);
            return ex;
        }
    }
}
