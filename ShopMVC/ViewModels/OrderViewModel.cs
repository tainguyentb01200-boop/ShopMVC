using ShopMVC.Models;

namespace ShopMVC.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; } = new Order();  // ← Đổi từ vw_OrderTotal
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();  // ← Đổi từ vw_OrderDetailsWithProduct

        // Trạng thái đơn hàng
        public string StatusText => Order.Status switch
        {
            "Pending" => "Chờ xử lý",
            "Approved" => "Đã duyệt",
            "Rejected" => "Đã từ chối",
            "Shipping" => "Đang giao hàng",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            _ => Order.Status ?? "Không xác định"
        };

        public string StatusColor => Order.Status switch
        {
            "Pending" => "warning",
            "Approved" => "info",
            "Rejected" => "danger",
            "Shipping" => "primary",
            "Completed" => "success",
            "Cancelled" => "secondary",
            _ => "secondary"
        };

        public bool CanCancel => Order.Status == "Pending";
    }

    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();  // ← Đổi từ vw_OrderTotal
        public string? StatusFilter { get; set; }
    }
}