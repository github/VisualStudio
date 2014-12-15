namespace GitHub
{
    public interface IApiClientFactory
    {
        IApiClient Create(HostAddress hostAddress);
    }
}
