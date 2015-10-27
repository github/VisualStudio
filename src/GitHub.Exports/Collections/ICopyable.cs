namespace GitHub.Collections
{
    public interface ICopyable<in T>
    {
        void CopyFrom(T other);
    }
}