using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using NullGuard;

namespace GitHub.UI
{
    /// <summary>
    /// Interaction logic for BusySpinner.xaml
    /// </summary>
    public partial class BusySpinner : UserControl
    {
        public BusySpinner()
        {
            InitializeComponent();

            storyboardRotate = (Storyboard) Resources["storyboardRotate"];
        }

        public void Pause()
        {
            // Calling Pause here would pause the storyboard, but leave the
            // inner animation running.  This wastes CPU and causes a flood
            // of messages (WM_TIMER, Dispatcher, etc).  Instead, the
            // storyboard needs to be stopped and restarted.
            //
            // The message flood from a runaway WPF animation looks like this:
            //
            //  <003110> 001400B6 P WM_TIMER wTimerID:2 tmprc:00000000
            //  <003111> 001400B6 P message:0xC13B [Registered:"DispatcherProcessQueue"] wParam:00000000 lParam:00000000
            //  <003112> 001400B6 P message:0xC13B [Registered:"DispatcherProcessQueue"] wParam:00000000 lParam:00000000
            //  <003113> 001400B6 P message:0xC13B [Registered:"DispatcherProcessQueue"] wParam:00000000 lParam:00000000
            //  <003114> 001400B6 P message:0xC13B [Registered:"DispatcherProcessQueue"] wParam:00000000 lParam:00000000
            //  <003115> 001F002A P message:0xC198 [Registered:"MilChannelNotify"] wParam:00000000 lParam:00000000
            //  <003116> 001F002A P message:0xC198 [Registered:"MilChannelNotify"] wParam:00000000 lParam:00000000
            storyboardRotate.Stop(this);
        }

        public void Resume()
        {
            storyboardRotate.Begin(this, true);
        }

        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            storyboardRotate.Begin(this, true);
        }

        private Storyboard storyboardRotate;
    }

    public class CanvasScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, [AllowNull] object parameter, [AllowNull] System.Globalization.CultureInfo culture)
        {
            double canvasWidthOrHeight = 120;
            double gridWidthOrHeight = (double)value;
            return gridWidthOrHeight / canvasWidthOrHeight;
        }

        [return: AllowNull]
        public object ConvertBack(object value, Type targetType, [AllowNull] object parameter, [AllowNull] System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
