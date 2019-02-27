using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitHub.Extensions.Reactive;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.UI
{
    public class UserErrorMessages : UserControl
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly IDisposable whenAnyShowingMessage;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly IDisposable whenAnyDataContext;

        public UserErrorMessages()
        {
            whenAnyShowingMessage = this.WhenAny(x => x.UserError, x => x.Value)
                .Select(x => x != null)
                .Subscribe(result =>
                {
                    IsShowingMessage = result;
                });

            whenAnyDataContext = this.WhenAny(x => x.UserError, x => x.Value)
                .WhereNotNull()
                .Subscribe(result =>
                {
                    DataContext = result;
                });
        }

        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(UserErrorMessages), new PropertyMetadata(new Thickness(0, 0, 8, 0)));
        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        public static readonly DependencyProperty MessageMarginProperty = DependencyProperty.Register("MessageMargin", typeof(Thickness), typeof(UserErrorMessages));
        public Thickness MessageMargin
        {
            get { return (Thickness)GetValue(MessageMarginProperty); }
            set { SetValue(MessageMarginProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Octicon), typeof(UserErrorMessages), new PropertyMetadata(Octicon.stop));
        public Octicon Icon
        {
            get { return (Octicon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(UserErrorMessages), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c))));
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty ErrorMessageFontWeightProperty = DependencyProperty.Register("ErrorMessageFontWeight", typeof(FontWeight), typeof(UserErrorMessages), new PropertyMetadata(FontWeights.Normal));
        public FontWeight ErrorMessageFontWeight
        {
            get { return (FontWeight)GetValue(ErrorMessageFontWeightProperty); }
            set { SetValue(ErrorMessageFontWeightProperty, value); }
        }

        public static readonly DependencyProperty IsShowingMessageProperty = DependencyProperty.Register("IsShowingMessage", typeof(bool), typeof(UserErrorMessages));
        public bool IsShowingMessage
        {
            get { return (bool)GetValue(IsShowingMessageProperty); }
            private set { SetValue(IsShowingMessageProperty, value); }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public static readonly DependencyProperty UserErrorProperty = DependencyProperty.Register("UserError", typeof(UserError), typeof(UserErrorMessages));
        public UserError UserError
        {
            get { return (UserError)GetValue(UserErrorProperty); }
            set { SetValue(UserErrorProperty, value); }
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "We're registering a handler for a type so this is appropriate.")]
        public IDisposable RegisterHandler<TUserError>(IObservable<bool> clearWhen) where TUserError : UserError
        {
            return UserError.RegisterHandler<TUserError>(userError =>
            {
                UserError = userError;
                return clearWhen
                    .Skip(1)
                    .Do(_ => UserError = null)
                    .Select(x => RecoveryOptionResult.CancelOperation);
            });
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
