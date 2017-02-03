using System;
using GitHub.Primitives;

namespace GitHub.Models
{
    public class ConnectionDetails : IEquatable<ConnectionDetails>
    {
        public ConnectionDetails(string hostAddress, string userName)
        {
            HostAddress = HostAddress.Create(hostAddress);
            UserName = userName;
        }

        public ConnectionDetails(HostAddress hostAddress, string userName)
        {
            HostAddress = hostAddress;
            UserName = userName;
        }

        public HostAddress HostAddress { get; }
        public string UserName { get; }

        public bool Equals(ConnectionDetails other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HostAddress.Equals(other.HostAddress) && string.Equals(UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((ConnectionDetails)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (HostAddress.GetHashCode()*397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(UserName);
            }
        }

        public static bool operator ==(ConnectionDetails left, ConnectionDetails right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConnectionDetails left, ConnectionDetails right)
        {
            return !Equals(left, right);
        }
    }
}
