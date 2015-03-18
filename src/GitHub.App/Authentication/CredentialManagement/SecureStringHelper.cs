using System;
using System.Runtime.InteropServices;
using System.Security;

namespace GitHub.Authentication.CredentialManagement
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SecureStringHelper
    {
        // Methods
        internal static SecureString CreateSecureString(string plainString)
        {

            SecureString str = new SecureString();
            if (!string.IsNullOrEmpty(plainString))
            {
                foreach (char c in plainString)
                {
                    str.AppendChar(c);
                }
            }
            /*
            fixed (char* str2 = plainString)
            {
                char* chPtr = str2;
                str = new SecureString(chPtr, plainString.Length);
                str.MakeReadOnly();
            }
            */
            return str;
        }

        internal static string CreateString(SecureString secureString)
        {
            string str;
            IntPtr zero = IntPtr.Zero;
            if ((secureString == null) || (secureString.Length == 0))
            {
                return string.Empty;
            }
            try
            {
                zero = Marshal.SecureStringToBSTR(secureString);
                str = Marshal.PtrToStringBSTR(zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(zero);
                }
            }
            return str;
        }
    }


}
