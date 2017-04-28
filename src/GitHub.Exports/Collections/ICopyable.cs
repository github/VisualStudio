namespace GitHub.Collections
{
    /// <summary>
    /// Allows a type instance to copy data from another
    /// instance. Similar to a Clone action, but without
    /// creating a new instance.
    /// </summary>
    /// <typeparam name="T">Type that implements CopyFrom</typeparam>
    public interface ICopyable<in T>
    {
        void CopyFrom(T other);
    }
}