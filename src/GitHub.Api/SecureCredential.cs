using System.Security;

namespace GitHub.Api
{
    public sealed class SecureCredential
    {
        public static readonly SecureCredential Anonymous = new SecureCredential("", null);
        public string Username { get; }
        public SecureString Password { get; }

        public SecureCredential(string username, SecureString password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
}