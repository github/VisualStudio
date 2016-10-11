using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Xunit;

namespace UnitTests.GitHub.App.Models
{
    public class AccountModelTests : TestBaseClass
    {
        [Fact]
        public void CopyFromDoesNotLoseAvatar()
        {
            //A function that will return this image in an observable after X seconds
            var userImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            var orgImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");

            Func<int, BitmapImage, IObservable<BitmapImage>> generateObservable = (seconds, image) =>
            {
                return Observable.Generate(
                    initialState: 0, 
                    condition: i => i < 1, 
                    iterate: state => state + 1,
                    resultSelector: i => image, 
                    timeSelector: i => TimeSpan.FromSeconds(seconds));
            };

            var evt = new ManualResetEvent(false);

            //Creating an initial account with an observable that returns immediately
            const string login = "foo";
            const int initialOwnedPrivateRepositoryCount = 1;

            var initialAccount = new Account(login, true, false, initialOwnedPrivateRepositoryCount, 0, generateObservable(0, userImage));

            //Creating the test collection
            var col = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(), OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            col.Subscribe(account =>
            {
                evt.Set();
            }, () => { });

            //Adding that account to the TrackingCollection
            col.AddItem(initialAccount);

            //Waiting for the collection add the item
            evt.WaitOne();
            evt.Reset();

            //Checking some initial properties
            Assert.Equal(login, col[0].Login);
            Assert.Equal(initialOwnedPrivateRepositoryCount, col[0].OwnedPrivateRepos);

            //Demonstrating that the avatar is present
            Assert.NotNull(col[0].Avatar);
            Assert.Equal(true, BitmapImageExtensions.IsEqual(col[0].Avatar, userImage));

            //Creating an observable that will return in one second
            var updatedBitmapSourceObservable = generateObservable(1, orgImage);

            //Creating an account update with an observable
            const int updatedOwnedPrivateRepositoryCount = 2;

            var updatedAccount = new Account(login, true, false, updatedOwnedPrivateRepositoryCount, 0, updatedBitmapSourceObservable);

            //Adding a listener to check for the changing of the Avatar property
            initialAccount.Changed.Subscribe(args =>
            {
                if (args.PropertyName == "Avatar")
                {
                    evt.Set();
                }
            });

            //Updating the accout in the collection
            col.AddItem(updatedAccount);

            //Waiting for the collection to process the update
            evt.WaitOne();
            evt.Reset();

            //Waiting for the delayed bitmap image observable
            evt.WaitOne();
            evt.Reset();

            //Login is the id, so that should be the same
            Assert.Equal(login, col[0].Login);

            //CopyFrom() should have updated this field
            Assert.Equal(updatedOwnedPrivateRepositoryCount, col[0].OwnedPrivateRepos);

            //CopyFrom() should not cause a race condition here
            Assert.NotNull(col[0].Avatar);
            Assert.Equal(true, BitmapImageExtensions.IsEqual((col[0].Avatar as BitmapImage), orgImage));
        }
    }

    public static class BitmapImageExtensions
    {
        //http://stackoverflow.com/questions/15558107/quickest-way-to-compare-two-bitmapimages-to-check-if-they-are-different-in-wpf

        public static bool IsEqual(BitmapSource image1, BitmapSource image2)
        {
            if (image1 == null || image2 == null)
            {
                return false;
            }
            return ToBytes(image1).SequenceEqual(ToBytes(image2));
        }

        public static byte[] ToBytes(BitmapSource image)
        {
            byte[] data = new byte[] { };
            if (image != null)
            {
                try
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        data = ms.ToArray();
                    }
                    return data;
                }
                catch (Exception ex)
                {
                }
            }
            return data;
        }
    }
}
