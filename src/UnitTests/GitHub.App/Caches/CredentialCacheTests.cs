using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Caches;
using Xunit;

public class CredentialCacheTests : TestBaseClass
{
    public class TheGetObjectMethod
    {
        [Fact]
        public async Task RetrievesValueWithAlternateKeys()
        {
            using (var credentialCache = new CredentialCache())
            {
                try
                {
                    var credential = Tuple.Create("somebody", "somebody's secret");
                    await credentialCache.InsertObject("git:key", credential);

                    var retrieved = await credentialCache.GetObject<Tuple<string, string>>("key");

                    Assert.Equal("somebody", retrieved.Item1);
                    Assert.Equal("somebody's secret", retrieved.Item2);

                    var retrieved2 = await credentialCache.GetObject<Tuple<string, string>>("git:key/");

                    Assert.Equal("somebody", retrieved2.Item1);
                    Assert.Equal("somebody's secret", retrieved2.Item2);

                    var retrieved3 = await credentialCache.GetObject<Tuple<string, string>>("login:key/");

                    Assert.Equal("somebody", retrieved3.Item1);
                    Assert.Equal("somebody's secret", retrieved3.Item2);
                }
                finally
                {
                    await credentialCache.Invalidate("git:key");
                }
            }
        }

        [Fact]
        public async Task ThrowsObservableInvalidOperationExceptionWhenRetrievingSomethingNotATuple()
        {
            using (var credentialCache = new CredentialCache())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await credentialCache.GetObject<string>("_"));
            }
        }

        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    async () => await credentialCache.GetObject<Tuple<string, string>>("_"));
            }
        }
    }

    public class TheInsertObjectMethod
    {
        [Fact]
        public async Task StoresCredentialForKey()
        {
            using (var credentialCache = new CredentialCache())
            {
                try
                {
                    var credential = Tuple.Create("somebody", "somebody's secret");

                    await credentialCache.InsertObject("key", credential);

                    var retrieved = await credentialCache.GetObject<Tuple<string, string>>("key");
                    Assert.Equal("somebody", retrieved.Item1);
                    Assert.Equal("somebody's secret", retrieved.Item2);
                }
                finally
                {
                    await credentialCache.Invalidate("key");
                }
            }
        }

        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    async () => await credentialCache.InsertObject("_", new object()));
            }
        }
    }

    public class TheInsertMethod
    {
        [Fact]
        public async Task ThrowsInvalidOperationException()
        {
            using (var credentialCache = new CredentialCache())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await credentialCache.Insert("key", new byte[] {}));
            }
        }
    }

    public class TheGetMethod
    {
        [Fact]
        public async Task RetrievesPasswordAsUnicodeBytes()
        {
            using (var credentialCache = new CredentialCache())
            {
                try
                {
                    var credential = Tuple.Create("somebody", "somebody's secret");
                    await credentialCache.InsertObject("key", credential);

                    var retrieved = await credentialCache.Get("key");

                    Assert.Equal("somebody's secret", Encoding.Unicode.GetString(retrieved));
                }
                finally
                {
                    await credentialCache.Invalidate("key");
                }
            }
        }

        [Fact]
        public async Task ThrowsObservableKeyNotFoundExceptionWhenKeyNotFound()
        {
            using (var credentialCache = new CredentialCache())
            {
                await Assert.ThrowsAsync<KeyNotFoundException>(async () => await credentialCache.Get("unknownkey"));
            }
        }

        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    async () => await credentialCache.Get("_"));
            }
        }
    }

    public class TheInvalidateMethod
    {
        [Fact]
        public async Task InvalidatesTheCredential()
        {
            using (var credentialCache = new CredentialCache())
            {
                try
                {
                    var credential = Tuple.Create("somebody", "somebody's secret");
                    await credentialCache.InsertObject("key", credential);
                }
                finally
                {
                    await credentialCache.Invalidate("key");
                }

                await Assert.ThrowsAsync<KeyNotFoundException>(async () => await credentialCache.Get("unknownkey"));
            }
        }

        [Fact]
        public async Task ThrowsKeyNotFoundExceptionWhenKeyNotFound()
        {
            using (var credentialCache = new CredentialCache())
            {
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    async () => await credentialCache.Invalidate("git:_"));
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    async () => await credentialCache.Invalidate("_"));
            }
        }

        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    async () => await credentialCache.Invalidate("_"));
            }
        }
    }

    public class TheInvalidateObjectMethod
    {
        [Theory]
        [InlineData("key")]
        [InlineData("key/")]
        [InlineData("git:key")]
        public async Task InvalidatesTheCredential(string key)
        {
            using (var credentialCache = new CredentialCache())
            {
                try
                {
                    var credential = Tuple.Create("somebody", "somebody's secret");
                    await credentialCache.InsertObject("key", credential);
                }
                finally
                {
                    await credentialCache.InvalidateObject<Tuple<string, string>>(key);

                    await Assert.ThrowsAsync<KeyNotFoundException>(async () => await credentialCache.Get("unknownkey"));
                }
            }
        }

        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(
                    async () => await credentialCache.InvalidateObject<Tuple<string, string>>("_"));
            }
        }

        [Fact]
        public async Task ThrowsKeyNotFoundExceptionWhenKeyNotFound()
        {
            using (var credentialCache = new CredentialCache())
            {
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    async () => await credentialCache.InvalidateObject<Tuple<string, string>>("git:_"));
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    async () => await credentialCache.InvalidateObject<Tuple<string, string>>("_"));
            }
        }
    }

    public class TheFlushMethod
    {
        [Fact]
        public async Task ThrowsObjectDisposedExceptionWhenDisposed()
        {
            using (var credentialCache = new CredentialCache())
            {
                await credentialCache.Flush();

                credentialCache.Dispose();
                await Assert.ThrowsAsync<ObjectDisposedException>(async () => await credentialCache.Flush());
            }
        }
    }

    public class TheDisposeMethod
    {
        [Fact]
        public void SignalsShutdown()
        {
            bool shutdown = false;
            using (var credentialCache = new CredentialCache())
            {
                credentialCache.Shutdown.Subscribe(_ => shutdown = true);
            }
            Assert.True(shutdown);
        }
    }

    public class MethodsNotImplementedOnPurpose
    {
        [Fact]
        public void ThrowNotImplementedException()
        {
            using (var credentialCache = new CredentialCache())
            {
                Assert.Throws<NotImplementedException>(() => credentialCache.GetAllKeys());
                Assert.Throws<NotImplementedException>(() => credentialCache.GetCreatedAt(""));
                Assert.Throws<NotImplementedException>(() => credentialCache.InvalidateAll());
                Assert.Throws<NotImplementedException>(() => credentialCache.InvalidateAllObjects<object>());
                Assert.Throws<NotImplementedException>(() => credentialCache.Vacuum());
                Assert.Throws<NotImplementedException>(() => credentialCache.GetAllObjects<object>());
                Assert.Throws<NotImplementedException>(() => credentialCache.GetObjectCreatedAt<object>(""));
            }
        }
    }
}
