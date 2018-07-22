using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.ViewModels;
using NUnit.Framework;

namespace GitHub.App.UnitTests.ViewModels
{
    public class UserFilterViewModelTests
    {
        [Test]
        public void Accessing_Users_Load_Users()
        {
            var target = CreateTarget();

            Assert.That(target.Users.Count, Is.EqualTo(5));
        }

        [Test]
        public void Setting_Filter_Adds_Ersatz_User()
        {
            var target = CreateTarget();
            var view = (ListCollectionView)target.UsersView;

            target.Filter = "grok";

            Assert.That(target.Users.Count, Is.EqualTo(6));
            Assert.That(target.Users.Last().Login, Is.EqualTo("grok"));
            Assert.That(((IActorViewModel)view.GetItemAt(0)).Login, Is.EqualTo("grok"));
        }

        [Test]
        public void Changing_Filter_Updates_Ersatz_User()
        {
            var target = CreateTarget();
            var view = (ListCollectionView)target.UsersView;

            target.Filter = "grok";

            Assert.That(target.Users.Count, Is.EqualTo(6));
            Assert.That(target.Users.Last().Login, Is.EqualTo("grok"));

            target.Filter = "shan";

            Assert.That(target.Users.Count, Is.EqualTo(6));
            Assert.That(target.Users.Last().Login, Is.EqualTo("shan"));
        }

        [Test]
        public void Changing_Filter_To_Existing_User_Removes_Ersatz_User()
        {
            var target = CreateTarget();
            var view = (ListCollectionView)target.UsersView;

            target.Filter = "grok";

            Assert.That(target.Users.Count, Is.EqualTo(6));
            Assert.That(target.Users.Last().Login, Is.EqualTo("grok"));

            target.Filter = "shana";

            Assert.That(target.Users.Count, Is.EqualTo(5));
        }

        [Test]
        public void Selecting_User_Clears_Filter()
        {
            var target = CreateTarget();

            target.Filter = "grok";
            target.Selected = target.Users[1];

            Assert.Null(target.Filter);
        }

        static UserFilterViewModel CreateTarget(UserFilterViewModel.LoadPageDelegate load = null)
        {
            Task<Page<ActorModel>> LoadPage(string after) => Task.FromResult(new Page<ActorModel>
            {
                TotalCount = 5,
                Items = new[]
                {
                    new ActorModel { Login = "grokys" },
                    new ActorModel { Login = "jcansdale" },
                    new ActorModel { Login = "meaghanlewis" },
                    new ActorModel { Login = "shana" },
                    new ActorModel { Login = "StanleyGoldman" },
                },
            });

            load = load ?? LoadPage;

            return new UserFilterViewModel(load);
        }
    }
}
