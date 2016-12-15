namespace GitHub.ViewModels
{
    /// <summary>
    /// Simple container for complex objects that we stuff
    /// into dropdowns and the likes
    /// </summary>
    public abstract class ViewModelItemContainer
    {
        public string Name;
        public override string ToString()
        {
            return Name;
        }
    }
}