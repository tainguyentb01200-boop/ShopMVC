using ShopMVC.Models;
using ShopMVC.Services.Interfaces;

namespace ShopMVC.ViewModels
{
    public class DashboardViewModel
    {
        public AdminDashboardStats Stats { get; set; } = new AdminDashboardStats();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopProductReport> TopProducts { get; set; } = new List<TopProductReport>();
        public List<CategoryStats> CategoryStats { get; set; } = new List<CategoryStats>();


    }

    public class AdminProductViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }
        
        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
    }

    public class AdminOrderViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public string? StatusFilter { get; set; }
        
        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
    }

    public class AdminUserViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public string? RoleFilter { get; set; }
        public string? SearchTerm { get; set; }
    }
}
