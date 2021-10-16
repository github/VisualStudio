using System;
using GitHub.Primitives;

namespace GitHub.Models
{
    /// <summary>
    /// Represents details about a connection stored in an <see cref="IConnectionCache"/>.
    /// </summary>
    public struct ConnectionDetails : IEquatable<ConnectionDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDetails"/> struct.
        /// </summary>
        /// <param name="hostAddress">The address of the host.</param>
        /// <param name="userName">The username for the host.</param>
        public ConnectionDetails(string hostAddress, string userName)
        {
            HostAddress = HostAddress.Create(hostAddress);
            UserName = userName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDetails"/> struct.
        /// </summary>
        /// <param name="hostAddress">The address of the host.</param>
        /// <param name="userName">The username for the host.</param>
        public ConnectionDetails(HostAddress hostAddress, string userName)
        {
            HostAddress = hostAddress;
            UserName = userName;
        }

        /// <summary>
        /// Gets the address of the host.
        /// </summary>
        public HostAddress HostAddress { get; }

        /// <summary>
        /// Gets the username for the host.
        /// </summary>
        public string UserName { get; }

        public bool Equals(ConnectionDetails other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return HostAddress.Equals(other.HostAddress) &&
                string.Equals(UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is ConnectionDetails && Equals((ConnectionDetails)obj);
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
