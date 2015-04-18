using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GitHub.UI
{
    [TemplatePart(Name = "R0"), TemplatePart(Name = "R1"), TemplatePart(Name = "R2"), TemplatePart(Name = "R3"), TemplatePart(Name = "R4")]
    public class GitHubProgressBar : RangeBase
    {
        public static readonly DependencyProperty IndicatorWidthProperty =
            DependencyProperty.Register("IndicatorWidth", typeof(double), typeof(GitHubProgressBar), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(GitHubProgressBar), new FrameworkPropertyMetadata(false, OnIndeterminateChanged));

        readonly Storyboard iStoryboard = new Storyboard { Duration = TimeSpan.FromSeconds(4.4), RepeatBehavior = RepeatBehavior.Forever };
        readonly LinearDoubleKeyFrame kf1 = new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
        readonly EasingDoubleKeyFrame kf2 = new EasingDoubleKeyFrame(33, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))) { EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut } };
        readonly LinearDoubleKeyFrame kf3 = new LinearDoubleKeyFrame(66, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.0)));
        readonly EasingDoubleKeyFrame kf4 = new EasingDoubleKeyFrame(100, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.5))) { EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn } };

        readonly DoubleAnimation progressAnimation = new DoubleAnimation(0, 0, new Duration(TimeSpan.FromSeconds(0.2))) { EasingFunction = new ExponentialEase { Exponent = 1, EasingMode = EasingMode.EaseInOut } };
        readonly Storyboard progressStoryboard = new Storyboard();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Static constructor does more than just initialize static fields.")]
        static GitHubProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GitHubProgressBar), new FrameworkPropertyMetadata(typeof(GitHubProgressBar)));
            MaximumProperty.OverrideMetadata(typeof(GitHubProgressBar), new FrameworkPropertyMetadata(100.0));
        }

        public GitHubProgressBar()
        {
            progressStoryboard.Children.Add(progressAnimation);
            Storyboard.SetTarget(progressAnimation, this);
            Storyboard.SetTargetProperty(progressAnimation, new PropertyPath(IndicatorWidthProperty));

            IsVisibleChanged += (s, e) => StartIndeterminateAnimation();
        }

        public double IndicatorWidth
        {
            get { return (double)GetValue(IndicatorWidthProperty); }
            set { SetValue(IndicatorWidthProperty, value); }
        }

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            for (int i = 0; i < 5; i++)
            {
                var r = GetTemplateChild(string.Format(CultureInfo.InvariantCulture, "R{0}", i)) as FrameworkElement;

                if (r == null) throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "R{0}", i));

                r.RenderTransform = new TranslateTransform();

                // Horizontal (x) animation
                var a = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(i*0.2) };
                a.KeyFrames.Add(kf1);
                a.KeyFrames.Add(kf2);
                a.KeyFrames.Add(kf3);
                a.KeyFrames.Add(kf4);

                iStoryboard.Children.Add(a);
                Storyboard.SetTarget(a, r);
                Storyboard.SetTargetProperty(a, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                // Opacity animation for moving in and out of view
                var b = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(i*0.2) };
                b.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                b.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.5))));

                iStoryboard.Children.Add(b);
                Storyboard.SetTarget(b, r);
                Storyboard.SetTargetProperty(b, new PropertyPath(OpacityProperty));
            }

            StartIndeterminateAnimation();
        }

        static void OnIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressBar = (GitHubProgressBar)d;
            VisualStateManager.GoToState(progressBar, progressBar.IsIndeterminate ? "Indeterminate" : "Determinate", true);

            progressBar.StartIndeterminateAnimation();
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            SetIndicatorWidth();
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            SetIndicatorWidth();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            SetIndicatorWidth();
            StartIndeterminateAnimation();
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            SetIndicatorWidth();
        }

        void SetIndicatorWidth()
        {
            if (IsIndeterminate) return;

            if (Value <= 0)
            {
                progressStoryboard.Remove();
                IndicatorWidth = 0;
                return;
            }

            var min = Minimum;
            var max = Maximum;
            var value = Value;
            var scale = (max <= min) ? 1.0 : ((value - min)/(max - min));

            progressAnimation.From = IndicatorWidth;
            progressAnimation.To = scale*ActualWidth;

            progressStoryboard.Begin();
        }

        void StartIndeterminateAnimation()
        {
            if (iStoryboard.Children.Count == 0) return; // haven't applied template yet

            iStoryboard.Stop();
            if (!IsIndeterminate || !IsVisible || ActualWidth <= 0) return;

            kf1.Value = 0;
            kf2.Value = ActualWidth/3.0;
            kf3.Value = ActualWidth/3.0*2;
            kf4.Value = ActualWidth;

            iStoryboard.Begin();
        }
    }
}
