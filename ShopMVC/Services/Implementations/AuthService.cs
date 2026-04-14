using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using BCrypt.Net;

namespace ShopMVC.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null) return null;

            // Verify password
            if (!VerifyPassword(password, user.Password))
                return null;

            return user;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                if (await UsernameExistsAsync(user.Username))
                    return false;

                // Kiểm tra email đã tồn tại
                if (!string.IsNullOrEmpty(user.Email) && await EmailExistsAsync(user.Email))
                    return false;

                // Hash password
                user.Password = HashPassword(password);
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> UpdateProfileAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.UserId);
                if (existingUser == null) return false;

                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Address = user.Address;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Verify old password
                if (!VerifyPassword(oldPassword, user.Password))
                    return false;

                // Update password
                user.Password = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}