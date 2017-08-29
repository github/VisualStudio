using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GitHub.InlineReviews.Views
{
    public class DiffView : StackPanel
    {
        static readonly Brush AddedBrush = new SolidColorBrush(Color.FromRgb(0xD7, 0xE3, 0xBC));
        static readonly Brush DeletedBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x99));

        public static readonly DependencyProperty DiffProperty =
            DependencyProperty.Register(
                nameof(Diff),
                typeof(string),
                typeof(DiffView),
                new PropertyMetadata(DiffChanged));

        public string Diff
        {
            get { return (string)GetValue(DiffProperty); }
            set { SetValue(DiffProperty, value); }
        }

        void UpdateContents()
        {
            Children.Clear();

            if (Diff != null)
            {
                using (var reader = new StringReader(Diff))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = line;
                        
                        if (line.Length > 0)
                        {
                            switch (line[0])
                            {
                                case '+':
                                    textBlock.Background = AddedBrush;
                                    break;
                                case '-':
                                    textBlock.Background = DeletedBrush;
                                    break;
                            }
                        }

                        Children.Add(textBlock);
                    }
                }
            }
        }

        static void DiffChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((DiffView)sender).UpdateContents();
        }
    }
}
