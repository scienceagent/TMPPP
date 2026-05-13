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

        /// <summary>
        /// Register a new user in the system.
        /// </summary>
        Task<bool> RegisterAsync(string username, string password, string role, string fullName, string email, string phone);
    }
}
