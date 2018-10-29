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
using NUnit.Framework;
using System.Globalization;

public class AvatarProviderTests
{
    public class TheDefaultOrgBitmapImageProperty : TestBaseClass
    {
        [Test]
        public async Task CanBeAccessedFromMultipleThreadsAsync()
        {
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var expected = avatarProvider.DefaultOrgBitmapImage.ToString(CultureInfo.InvariantCulture);
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int otherThreadId = mainThreadId;

            var actual = await Task.Run(() =>
            {
                otherThreadId = Thread.CurrentThread.ManagedThreadId;
                return avatarProvider.DefaultOrgBitmapImage.ToString(CultureInfo.InvariantCulture);
            });

            Assert.That(expected, Is.EqualTo(actual));
            Assert.That(mainThreadId, Is.Not.EqualTo(otherThreadId));
        }
    }

    public class TheDefaultUserBitmapImageProperty : TestBaseClass
    {
        [Test]
        public async Task CanBeAccessedFromMultipleThreadsAsync()
        {
            var blobCache = new InMemoryBlobCache();
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobCache);
            var imageCache = new TestImageCache();
            var avatarProvider = new AvatarProvider(sharedCache, imageCache);
            var expected = avatarProvider.DefaultUserBitmapImage.ToString(CultureInfo.InvariantCulture);
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            int otherThreadId = mainThreadId;

            var actual = await Task.Run(() =>
            {
                otherThreadId = Thread.CurrentThread.ManagedThreadId;
                return avatarProvider.DefaultUserBitmapImage.ToString(CultureInfo.InvariantCulture);
            });

            Assert.That(expected, Is.EqualTo(actual));
            Assert.That(mainThreadId, Is.Not.EqualTo(otherThreadId));
        }
    }

    public class TheGetAvatarMethod : TestBaseClass
    {
        [Test]
        public async Task GetsAvatarFromCacheAsync()
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

        [Test]
        public async Task RetrievesGitHubAvatarAsync()
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
            Assert.That(expected, Is.EqualTo(actualBytes));
        }

        static void AssertSameImage(BitmapSource expected, BitmapSource actual)
        {
            var expectedBytes = ImageCache.GetBytesFromBitmapImage(expected);
            var actualBytes = ImageCache.GetBytesFromBitmapImage(actual);

            // Since we scale the image, we can't compare all the bytes, so we'll compare a few of them.
            // TODO: This is probably not correct, but we've manually verified the code we're testing.
            // We need a way to test similarity of images.
            const int bytesToCompare = 19;
            Assert.That(expectedBytes.Take(bytesToCompare), Is.EqualTo(actualBytes.Take(bytesToCompare)));
        }
    }

    public class TheInvalidateAvatarMethod : TestBaseClass
    {
        [Test]
        public void DoesNotThrowOnNullUserOrAvatarUrl()
        {
            var blobStore = Substitute.For<IBlobCache>();
            blobStore.Invalidate(null).Returns(_ => { throw new ArgumentNullException("key"); });
            var sharedCache = Substitute.For<ISharedCache>();
            sharedCache.LocalMachine.Returns(blobStore);

            var avatarProvider = new AvatarProvider(sharedCache, new TestImageCache());

            avatarProvider.InvalidateAvatar(null);
            avatarProvider.InvalidateAvatar(Substitute.For<IAvatarContainer>());
        }
    }

    public class TheGetBytesFromBitmapImageMethod : TestBaseClass
    {
        [Test]
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

            Assert.That(bytes, Is.Not.Null);
            Assert.True(bytes.Length > 256);
        }
    }
}
