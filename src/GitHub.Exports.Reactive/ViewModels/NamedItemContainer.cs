namespace GitHub.ViewModels
{
    /// <summary>
    /// Simple container for complex objects that we stuff
    /// into dropdowns and the likes
    /// </summary>
    public abstract class NamedItemContainer
    {
        protected NamedItemContainer(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public override string ToString()
        {
            return Name;
        }
    }
}