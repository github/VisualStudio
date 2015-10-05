namespace GitHub.Helpers
{
    public interface ICopyable<in T>
    {
        void CopyFrom(T other);
    }
}