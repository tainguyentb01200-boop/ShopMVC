using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using ShopMVC.ViewModels;

namespace ShopMVC.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET DASHBOARD STATS
        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStats
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount ?? 0),

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.Status == "Pending"),

                LowStockProducts = await _context.Products
                    .CountAsync(p => p.Stock < 10 && p.IsActive),

                NewOrdersToday = await _context.Orders
                    .CountAsync(o => o.OrderDate.Date == DateTime.Today),

                RevenueToday = await _context.Orders
                    .Where(o => o.OrderDate.Date == DateTime.Today && o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount ?? 0)
            };

            return stats;
        }

        // GET TOP PRODUCTS
        // GET TOP PRODUCTS
        public async Task<List<TopProductReport>> GetTopProductsAsync(int count)
        {
            var topProducts = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.Status == "Completed")
                .GroupBy(od => new { od.ProductId, od.Product.ProductName, od.Product.Image })
                .Select(g => new TopProductReport
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    Image = g.Key.Image,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        // GET CATEGORY STATS
        public async Task<List<CategoryStats>> GetCategoryStatsAsync()
        {
            var categoryStats = await _context.Categories
                .Select(c => new CategoryStats
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    TotalRevenue = c.Products
                        .SelectMany(p => p.OrderDetails)
                        .Where(od => od.Order.Status == "Completed")  // ← CHỈ THÊM DÒNG NÀY
                        .Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToListAsync();

            return categoryStats;
        }

        // GET SALES REPORT
        public async Task<SalesReport> GetSalesReportAsync(DateTime fromDate, DateTime toDate)
        {
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            var report = new SalesReport
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount ?? 0),
                CompletedOrders = orders.Count(o => o.Status == "Completed"),
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),

                DailySales = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySales
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.TotalAmount ?? 0)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return report;
        }

        // GET ALL USERS
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        // GET USER DETAIL
        public async Task<User?> GetUserDetailAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.OrderUsers)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // DEACTIVATE USER
        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // ACTIVATE USER
        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}