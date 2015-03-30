using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    public class Connection : IConnection
    {
        public Connection(HostAddress hostAddress, string userName)
        {
            HostAddress = hostAddress;
            Username = userName;
        }

        public HostAddress HostAddress { get; private set; }
        public string Username { get; private set; }
    }
}
