using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models.User;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Services
{
    public class AuthenticationService : IAuthService
    {
        public async Task<bool> AuthenticateAsync(string username, SecureString password, string role)
        {
            await Task.Delay(100).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(username) || password == null || password.Length == 0)
                return false;

            IntPtr unmanaged = IntPtr.Zero;
            try
            {
                unmanaged = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(password);
                var plainPassword = System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanaged) ?? string.Empty;

                using (var context = new AppDbContext())
                {
                    var user = await context.Users
                        .Where(u => u.Username == username && u.Password == plainPassword)
                        .FirstOrDefaultAsync();

                    if (user == null) return false;

                    // If user is Admin, check assigned role field
                    if (user is Admin admin)
                    {
                        return admin.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
                    }

                    // Otherwise if it matches the DB, we consider it valid for now
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (unmanaged != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string role, string fullName, string email, string phone)
        {
            await Task.Delay(100);

            try
            {
                using (var context = new AppDbContext())
                {
                    // Ensure DB exists (just in case)
                    context.Database.EnsureCreated();

                    // Check if username already exists
                    bool exists = await context.Users.AnyAsync(u => u.Username == username);
                    if (exists) throw new InvalidOperationException($"Username '{username}' is already taken.");

                    User newUser;
                    string id = Guid.NewGuid().ToString();

                    if (role.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                    {
                        newUser = new Guest(id, fullName, email, phone, username, password, "Default", "0000");
                    }
                    else
                    {
                        newUser = new Admin(id, fullName, email, phone, username, password, role, "Operational", new System.Collections.Generic.List<string>());
                    }

                    context.Users.Add(newUser);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Re-throw to be caught by the ViewModel's catch block
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }
    }
}
