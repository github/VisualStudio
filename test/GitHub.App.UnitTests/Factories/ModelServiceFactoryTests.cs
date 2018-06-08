using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.Factories
{
    public class ModelServiceFactoryTests : TestBaseClass
    {
        public class TheCreateAsyncMethod
        {
            [Test]
            public async Task ShouldCreateDifferentModelServiceForDifferentHostAsync()
            {
                var target = CreateTarget();
                var instance1 = await target.CreateAsync(CreateConnection("https://github.com"));
                var instance2 = await target.CreateAsync(CreateConnection("https://another.com"));

                Assert.That(instance1, Is.Not.SameAs(instance2));
            }

            [Test]
            public async Task ShouldCreateDifferentModelServiceForDifferentConnectionsWithSameAddressAsync()
            {
                var target = CreateTarget();
                var instance1 = await target.CreateAsync(CreateConnection("https://github.com"));
                var instance2 = await target.CreateAsync(CreateConnection("https://github.com"));

                Assert.That(instance1, Is.Not.SameAs(instance2));
            }

            [Test]
            public async Task ShouldCacheModelServiceForHostAsync()
            {
                var target = CreateTarget();
                var connection = CreateConnection("https://github.com");
                var instance1 = await target.CreateAsync(connection);
                var instance2 = await target.CreateAsync(connection);

                Assert.That(instance1, Is.SameAs(instance2));
            }

            [Test]
            public async Task ShouldInsertUserAsync()
            {
                var hostCacheFactory = Substitute.For<IHostCacheFactory>();
                var target = CreateTarget(hostCacheFactory: hostCacheFactory);
                var connection = CreateConnection("https://github.com");
                var hostCache = await hostCacheFactory.Create(connection.HostAddress);
                var modelService = await target.CreateAsync(connection);

                hostCache.Received().Insert("GitHub.Caches.AccountCacheItem___user", Arg.Any<byte[]>());
            }
        }

        static ModelServiceFactory CreateTarget(
            IHostCacheFactory hostCacheFactory = null)
        {
            var apiClientFactory = Substitute.For<IApiClientFactory>();
            var graphQLClientFactory = Substitute.For<IGraphQLClientFactory>();
            var avatarProvider = Substitute.For<IAvatarProvider>();

            hostCacheFactory = hostCacheFactory ?? Substitute.For<IHostCacheFactory>();

            return new ModelServiceFactory(
                apiClientFactory,
                graphQLClientFactory,
                hostCacheFactory,
                avatarProvider);
        }

        static IConnection CreateConnection(string address, string login = "user")
        {
            var result = Substitute.For<IConnection>();
            var user = CreateOctokitUser(login, address);
            result.HostAddress.Returns(HostAddress.Create(address));
            result.User.Returns(user);
            return result;
        }
    }
}
