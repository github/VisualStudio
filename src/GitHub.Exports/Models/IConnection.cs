using GitHub.Primitives;

namespace GitHub.Models
{
    public interface IConnection
    {
        HostAddress HostAddress { get; }
        string Username{ get; }
    }
}
