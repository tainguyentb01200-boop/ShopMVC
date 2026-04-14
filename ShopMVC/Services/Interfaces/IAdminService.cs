using ShopMVC.Models;
using ShopMVC.ViewModels;

namespace ShopMVC.Services.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<List<TopProductReport>> GetTopProductsAsync(int count);
        Task<List<CategoryStats>> GetCategoryStatsAsync();

        // Reports
        Task<SalesReport> GetSalesReportAsync(DateTime fromDate, DateTime toDate);

        // User management
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserDetailAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
    }
}