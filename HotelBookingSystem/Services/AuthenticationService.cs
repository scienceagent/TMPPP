using System;
using System.Security;
using System.Threading.Tasks;

namespace HotelBookingSystem.Services
{
    /// <summary>
    /// Simple mock authentication service.
    /// Replace with real API/credential store integration.
    /// </summary>
    public class AuthenticationService : IAuthService
    {
        public async Task<bool> AuthenticateAsync(string username, SecureString password, string role)
        {
            // Simulate network latency
            await Task.Delay(120).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(username) || password == null || password.Length == 0)
                return false;

            IntPtr unmanaged = IntPtr.Zero;
            try
            {
                unmanaged = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(password);
                var plain = System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanaged) ?? string.Empty;

                // Mock credentials (demo only)
                if ((username == "admin" && plain == "admin") || (username == "staff" && plain == "staff"))
                    return true;

                return false;
            }
            finally
            {
                if (unmanaged != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
            }
        }
    }
}
