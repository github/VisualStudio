using System.Reactive.Linq;
using System.Windows;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.UI
{
    /// <summary>
    /// Interaction logic for AutoCompleteSuggestionView.xaml
    /// </summary>
    public partial class AutoCompleteSuggestionView : IViewFor<AutoCompleteSuggestion>
    {
        public AutoCompleteSuggestionView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Name, v => v.name.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Description, v => v.description.Text));

                var imageObservable = this.WhenAnyObservable(v => v.ViewModel.Image);
                d(imageObservable.WhereNotNull().BindTo(this, v => v.image.Source));
                d(imageObservable.Select(image => image != null).BindTo(this, v => v.image.Visibility));
                d(imageObservable.Select(image => image != null)
                    .Select(visible => new Thickness((visible ? 5 : 0), 0, 0, 0))
                    .BindTo(this, v => v.suggestionText.Margin));
            });
        }

        public AutoCompleteSuggestion ViewModel
        {
            get { return (AutoCompleteSuggestion)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register("ViewModel", typeof(AutoCompleteSuggestion), typeof(AutoCompleteSuggestionView),
            new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (AutoCompleteSuggestion)value; }
        }
    }
}
