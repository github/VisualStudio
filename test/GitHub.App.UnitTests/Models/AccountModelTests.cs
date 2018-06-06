using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Media.Imaging;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using NUnit.Framework;
using Serilog;

namespace UnitTests.GitHub.App.Models
{
    public class AccountModelTests : TestBaseClass
    {
        [Test]
        public void CopyFromDoesNotLoseAvatar()
        {
            var userImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            var orgImage = AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");

            var initialBitmapImageSubject = new Subject<BitmapImage>();

            var collectionEvent = new ManualResetEvent(false);
            var avatarPropertyEvent = new ManualResetEvent(false);

            //Creating an initial account with an observable that returns immediately
            const string login = "foo";
            const int initialOwnedPrivateRepositoryCount = 1;

            var initialAccount = new Account(login, true, false, initialOwnedPrivateRepositoryCount, 0, null, initialBitmapImageSubject);

            //Creating the test collection
            var col = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(), OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            col.Subscribe(account =>
            {
                collectionEvent.Set();
            }, () => { });

            //Adding that account to the TrackingCollection
            col.AddItem(initialAccount);

            //Waiting for the collection add the item
            collectionEvent.WaitOne();
            collectionEvent.Reset();

            //Checking some initial properties
            Assert.That(login, Is.EqualTo(col[0].Login));
            Assert.That(initialOwnedPrivateRepositoryCount, Is.EqualTo(col[0].OwnedPrivateRepos));

            //Demonstrating that the avatar is not yet present
            Assert.That(col[0].Avatar, Is.Null);

            //Adding a listener to check for the changing of the Avatar property
            initialAccount.Changed.Subscribe(args =>
            {
                if (args.PropertyName == "Avatar")
                {
                    avatarPropertyEvent.Set();
                }
            });

            //Providing the first avatar
            initialBitmapImageSubject.OnNext(userImage);
            initialBitmapImageSubject.OnCompleted();

            //Waiting for the avatar to be added
            avatarPropertyEvent.WaitOne();
            avatarPropertyEvent.Reset();

            //Demonstrating that the avatar is present
            Assert.That(col[0].Avatar, Is.Not.Null);
            Assert.True(BitmapSourcesAreEqual(col[0].Avatar, userImage));
            Assert.False(BitmapSourcesAreEqual(col[0].Avatar, orgImage));

            //Creating an account update
            const int updatedOwnedPrivateRepositoryCount = 2;
            var updatedBitmapImageSubject = new Subject<BitmapImage>();
            var updatedAccount = new Account(login, true, false, updatedOwnedPrivateRepositoryCount, 0, null, updatedBitmapImageSubject);

            //Updating the account in the collection
            col.AddItem(updatedAccount);

            //Waiting for the collection to process the update
            collectionEvent.WaitOne();
            collectionEvent.Reset();

            //Providing the second avatar
            updatedBitmapImageSubject.OnNext(orgImage);
            updatedBitmapImageSubject.OnCompleted();

            //Waiting for the delayed bitmap image observable
            avatarPropertyEvent.WaitOne();
            avatarPropertyEvent.Reset();

            //Login is the id, so that should be the same
            Assert.That(login, Is.EqualTo(col[0].Login));

            //CopyFrom() should have updated this field
            Assert.That(updatedOwnedPrivateRepositoryCount, Is.EqualTo(col[0].OwnedPrivateRepos));

            //CopyFrom() should not cause a race condition here
            Assert.That(col[0].Avatar, Is.Not.Null);
            Assert.True(BitmapSourcesAreEqual(col[0].Avatar, orgImage));
            Assert.False(BitmapSourcesAreEqual(col[0].Avatar, userImage));
        }

        public static bool BitmapSourcesAreEqual(BitmapSource image1, BitmapSource image2)
        {
            if (image1 == null || image2 == null)
            {
                return false;
            }

            return BitmapSourceToBytes(image1).SequenceEqual(BitmapSourceToBytes(image2));
        }

        public static byte[] BitmapSourceToBytes(BitmapSource image)
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
                    Log.Error(ex, "Error'");
                }
            }

            return data;
        }
    }
}
