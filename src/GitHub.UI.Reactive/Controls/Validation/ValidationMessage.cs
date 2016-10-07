using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using GitHub.Extensions.Reactive;
using GitHub.Validation;
using NullGuard;
using ReactiveUI;

namespace GitHub.UI
{
    public class ValidationMessage : UserControl
    {
        const double defaultTextChangeThrottle = 0.2;
        bool userHasInteracted;

        public ValidationMessage()
        {
            this.WhenAny(x => x.ReactiveValidator.ValidationResult, x => x.Value)
                .WhereNotNull()
                .Subscribe(result =>
                {
                    ShowError = result.IsValid == false;
                    Text = result.Message;
                });

            this.WhenAny(x => x.ValidatesControl, x => x.Value)
                .WhereNotNull()
                .Do(CreateBinding)
                .Select(control =>
                    Observable.Merge(
                        this.WhenAnyValue(x => x.ShowError).Where(x => userHasInteracted),
                        control.Events().TextChanged
                            .Throttle(TimeSpan.FromSeconds(ShowError ? defaultTextChangeThrottle : TextChangeThrottle),
                            RxApp.MainThreadScheduler)
                            .Select(_ => ShowError),
                        control.Events().LostFocus
                            .Select(_ => ShowError),
                        control.Events().LostFocus
                            .Where(__ => string.IsNullOrEmpty(ValidatesControl.Text))
                            .Select(_ => false)))
                .Switch()
                .Subscribe(ShowValidateError);
        }

        public static readonly DependencyProperty IsShowingMessageProperty = DependencyProperty.Register("IsShowingMessage", typeof(bool), typeof(ValidationMessage));
        public bool IsShowingMessage
        {
            get { return (bool)GetValue(IsShowingMessageProperty); }
            private set { SetValue(IsShowingMessageProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ValidationMessage));
        public string Text
        {
            [return: AllowNull]
            get { return (string)GetValue(TextProperty); }
            private set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ShowErrorProperty = DependencyProperty.Register("ShowError", typeof(bool), typeof(ValidationMessage));
        public bool ShowError
        {
            get { return (bool)GetValue(ShowErrorProperty); }
            set { SetValue(ShowErrorProperty, value); }
        }

        public static readonly DependencyProperty TextChangeThrottleProperty = DependencyProperty.Register("TextChangeThrottle", typeof(double), typeof(ValidationMessage), new PropertyMetadata(defaultTextChangeThrottle));
        public double TextChangeThrottle
        {
            get { return (double)GetValue(TextChangeThrottleProperty); }
            set { SetValue(TextChangeThrottleProperty, value); }
        }

        public static readonly DependencyProperty ValidatesControlProperty = DependencyProperty.Register("ValidatesControl", typeof(TextBox), typeof(ValidationMessage), new PropertyMetadata(default(TextBox)));
        public TextBox ValidatesControl
        {
            [return: AllowNull]
            get { return (TextBox)GetValue(ValidatesControlProperty); }
            set { SetValue(ValidatesControlProperty, value); }
        }

        public static readonly DependencyProperty ReactiveValidatorProperty = DependencyProperty.Register("ReactiveValidator", typeof(ReactivePropertyValidator), typeof(ValidationMessage));
        public ReactivePropertyValidator ReactiveValidator
        {
            [return: AllowNull]
            get { return (ReactivePropertyValidator)GetValue(ReactiveValidatorProperty); }
            set { SetValue(ReactiveValidatorProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Octicon), typeof(ValidationMessage), new PropertyMetadata(Octicon.stop));
        public Octicon Icon
        {
            [return: AllowNull]
            get { return (Octicon) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(ValidationMessage), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xe7, 0x4c, 0x3c))));
        public Brush Fill
        {
            [return: AllowNull]
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty ErrorAdornerTemplateProperty = DependencyProperty.Register("ErrorAdornerTemplate", typeof(string), typeof(ValidationMessage), new PropertyMetadata("validationTemplate"));
        [AllowNull]
        public string ErrorAdornerTemplate
        {
            [return: AllowNull]
            get { return (string)GetValue(ErrorAdornerTemplateProperty); }
            set { SetValue(ErrorAdornerTemplateProperty, value); }
        }

        void ShowValidateError(bool showError)
        {
            IsShowingMessage = showError;
            userHasInteracted = true;

            if (ValidatesControl == null || !IsAdornerEnabled()) return;

            var bindingExpression = ValidatesControl.GetBindingExpression(TextBox.TagProperty);
            if (bindingExpression == null) return;
            var opExpression = BindingOperations.GetBindingExpression(ValidatesControl, TextBox.TagProperty);
            if (opExpression == null) return;
            var validationError = new ValidationError(new ExceptionValidationRule(), opExpression);

            if (showError)
            {
                validationError.ErrorContent = Text;
                System.Windows.Controls.Validation.MarkInvalid(bindingExpression, validationError);
            }
            else
            {
                System.Windows.Controls.Validation.ClearInvalid(bindingExpression);
            }
        }

        void CreateBinding(TextBox textBox)
        {
            if (textBox == null || !IsAdornerEnabled())
            {
                return;
            }

            var template = FindResource(ErrorAdornerTemplate) as ControlTemplate;
            if (template != null)
                System.Windows.Controls.Validation.SetErrorTemplate(textBox, template);

            var validationBinding = new Binding { Source = textBox, Path = new PropertyPath("Tag") };
            validationBinding.NotifyOnValidationError = true;
            validationBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            validationBinding.ValidationRules.Add(new ExceptionValidationRule());
            textBox.SetBinding(TagProperty, validationBinding);
        }

        bool IsAdornerEnabled()
        {
            return !string.IsNullOrEmpty(ErrorAdornerTemplate)
                && !ErrorAdornerTemplate.Equals("None", StringComparison.OrdinalIgnoreCase);
        }
    }
}
