using System;
using System.Windows.Media;
using GitHub.Models;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for an issue/pull request label.
    /// </summary>
    public class LabelViewModel : ViewModelBase, ILabelViewModel
    {
        public LabelViewModel(LabelModel model)
            : this(model.Name, (Color)ColorConverter.ConvertFromString('#' + model.Color))
        {
        }

        public LabelViewModel(string name, Color color)
        {
            Name = name;
            BackgroundBrush = new SolidColorBrush(color);
            ForegroundBrush = Brushes.White;
        }

        public string Name { get; }
        public Brush BackgroundBrush { get; }
        public Brush ForegroundBrush { get; }
    }
}
 