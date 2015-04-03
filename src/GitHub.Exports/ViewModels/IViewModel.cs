using System.Windows.Input;

namespace GitHub.ViewModels
{
    public interface IViewModel
    {
        string Title { get; }
        ICommand Cancel { get; }
	}
}