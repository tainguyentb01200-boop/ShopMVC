namespace ShopMVC.Helpers
{
    public static class OrderStatusHelper
    {
        public static Dictionary<string, string> GetAvailableStatuses(string currentStatus)
        {
            var nextStatuses = new Dictionary<string, string>();

            switch (currentStatus)
            {
                case "Pending": // Chờ duyệt
                    nextStatuses.Add("Approved", "Đã duyệt (Approved)");
                    nextStatuses.Add("Rejected", "Từ chối (Rejected)");
                    nextStatuses.Add("Cancelled", "Hủy đơn (Cancelled)");
                    break;

                case "Approved": // Đã duyệt
                    nextStatuses.Add("Shipping", "Đang giao hàng (Shipping)");
                    nextStatuses.Add("Cancelled", "Hủy đơn (Cancelled)");
                    break;

                case "Shipping": // Đang giao
                    nextStatuses.Add("Completed", "Hoàn thành (Completed)");
                    nextStatuses.Add("Cancelled", "Hủy/Trả hàng (Cancelled)");
                    break;

                case "Completed": // Hoàn thành
                    // Không cho phép thay đổi, hoặc chỉ cho phép Refund (nếu có tính năng đó)
                    break;

                case "Cancelled": // Đã hủy
                case "Rejected":  // Đã từ chối
                    // Trạng thái kết thúc, có thể cho phép mở lại đơn (Pending) nếu cần
                    // nextStatuses.Add("Pending", "Mở lại đơn hàng");
                    break;
            }

            return nextStatuses;
        }

        // Hàm lấy màu sắc badge hiển thị (Giữ lại logic cũ của bạn nhưng đưa vào đây cho gọn)
        public static string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "warning",
                "Approved" => "info",
                "Shipping" => "primary",
                "Completed" => "success",
                "Cancelled" => "secondary",
                "Rejected" => "danger",
                _ => "secondary"
            };
        }

        // Hàm lấy tên hiển thị tiếng Việt
        public static string GetStatusVietnamese(string status)
        {
            return status switch
            {
                "Pending" => "Chờ duyệt",
                "Approved" => "Đã duyệt",
                "Shipping" => "Đang giao",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                "Rejected" => "Từ chối",
                _ => status
            };
        }
    }
}