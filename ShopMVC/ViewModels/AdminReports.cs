namespace ShopMVC.ViewModels
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int NewOrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
    }

    public class TopProductReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Image { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CategoryStats
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SalesReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<DailySales> DailySales { get; set; } = new();
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}