using System.Security;
using System.Threading.Tasks;

namespace HotelBookingSystem.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate a user with username, secure password and role.
        /// Returns true when authentication succeeds.
        /// </summary>
        Task<bool> AuthenticateAsync(string username, SecureString password, string role);
    }
}
