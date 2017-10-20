using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Akavache;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using UnitTests.Helpers;
using Xunit;

public class AvatarProviderTests
{
    public class TheDefaultOrgBitmapImageProperty : TestBaseClass
    {
        [Fact]
        public async Task CanBeAccessedFromMultipleThreads()
        {
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var expected = avatarProvider.DefaultOrgBitmapImage.ToString();
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int otherThreadId = mainThreadId;

            var actual = await Task.Run(() =>
            {
                otherThreadId = Thread.CurrentThread.ManagedThreadId;
                return avatarProvider.DefaultOrgBitmapImage.ToString();
            });

            Assert.Equal(expected, actual);
            Assert.NotEqual(mainThreadId, otherThreadId);
        }
    }

    public class TheDefaultUserBitmapImageProperty : TestBaseClass
    {
        [Fact]
        public async Task CanBeAccessedFromMultipleThreads()
        {
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var expected = avatarProvider.DefaultUserBitmapImage.ToString();
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int otherThreadId = mainThreadId;

            var actual = await Task.Run(() =>
            {
                otherThreadId = Thread.CurrentThread.ManagedThreadId;
                return avatarProvider.DefaultUserBitmapImage.ToString();
            });

            Assert.Equal(expected, actual);
            Assert.NotEqual(mainThreadId, otherThreadId);
        }
    }

    public class TheGetAvatarMethod : TestBaseClass
    {
        [Fact]
        public async Task GetsAvatarFromCache()
        {
            var expectedImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
            var avatarUrl = new Uri("https://avatars.githubusercontent.com/u/e?email=me@test.com&s=140");
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            await imageCache.SeedImage(avatarUrl, expectedImage, DateTimeOffset.MaxValue);
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var avatarContainer = Substitute.For<IAvatarContainer>();
            avatarContainer.AvatarUrl.Returns("https://avatars.githubusercontent.com/u/e?email=me@test.com&s=140");

            var retrieved = await avatarProvider.GetAvatar(avatarContainer);

            AssertSameImage(expectedImage, retrieved);
        }

        [Fact]
        public async Task RetrievesGitHubAvatar()
        {
            var expectedImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
            var avatarUrl = new Uri("https://avatars.githubusercontent.com/u/e?email=me@test.com&s=140");
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            await imageCache.SeedImage(avatarUrl, expectedImage, DateTimeOffset.MaxValue);
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var avatarContainer = Substitute.For<IAvatarContainer>();
            avatarContainer.AvatarUrl.Returns("https://avatars.githubusercontent.com/u/e?email=me@test.com&s=140");

            var retrieved = await avatarProvider.GetAvatar(avatarContainer);

            AssertSameImage(expectedImage, retrieved);
        }

        static void AssertSameImage(byte[] expected, BitmapSource imageSource)
        {
            var actualBytes = ImageCache.GetBytesFromBitmapImage(imageSource);
            Assert.Equal(expected, actualBytes);
        }

        static void AssertSameImage(BitmapSource expected, BitmapSource actual)
        {
            var expectedBytes = ImageCache.GetBytesFromBitmapImage(expected);
            var actualBytes = ImageCache.GetBytesFromBitmapImage(actual);

            // Since we scale the image, we can't compare all the bytes, so we'll compare a few of them.
            // TODO: This is probably not correct, but we've manually verified the code we're testing.
            // We need a way to test similarity of images.
            const int bytesToCompare = 19;
            Assert.Equal(expectedBytes.Take(bytesToCompare), actualBytes.Take(bytesToCompare));
        }
    }

    public class TheInvalidateAvatarMethod : TestBaseClass
    {
        [Fact]
        public void DoesNotThrowOnNullUserOrAvatarUrl()
        {
            var blobStore = Substitute.For<IBlobCache>();
            blobStore.Invalidate(null).Returns(_ => { throw new ArgumentNullException(); });
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobStore);

            var avatarProvider = new AvatarProvider(sharedCache, new TestImageCache());

            avatarProvider.InvalidateAvatar(null);
            avatarProvider.InvalidateAvatar(Substitute.For<IAvatarContainer>());
        }
    }

    public class TheGetBytesFromBitmapImageMethod : TestBaseClass
    {
        [Fact]
        public void GetsBytesFromImage()
        {
            var image = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");

            byte[] bytes;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                bytes = ms.ToArray();
            }

            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 256);
        }
    }
}
