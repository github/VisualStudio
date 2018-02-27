using System;
using System.Windows.Media;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    public class LabelViewModelDesigner : ILabelViewModel
    {
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public string Name { get; set; }
    }
}
