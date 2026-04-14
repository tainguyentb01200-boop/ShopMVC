using ShopMVC.Models;
using ShopMVC.ViewModels;

namespace ShopMVC.Services.Interfaces
{
    public interface IAuthService
    {
        // Đăng nhập
        Task<User?> LoginAsync(string username, string password);

        // Đăng ký
        Task<bool> RegisterAsync(User user, string password);

        // Kiểm tra username tồn tại
        Task<bool> UsernameExistsAsync(string username);

        // Kiểm tra email tồn tại
        Task<bool> EmailExistsAsync(string email);

        // Lấy thông tin user theo ID
        Task<User?> GetUserByIdAsync(int userId);

        // Cập nhật profile
        Task<bool> UpdateProfileAsync(User user);

        // Đổi mật khẩu
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        // Hash password
        string HashPassword(string password);

        // Verify password
        bool VerifyPassword(string password, string hashedPassword);
 
    }
}