using Microsoft.AspNetCore.Mvc;
using ShopMVC.Services.Interfaces;
using ShopMVC.Helpers;

namespace ShopMVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var orders = await _orderService.GetOrdersByUserAsync(userId);

            // DEBUG: Kiểm tra OrderDetails
            foreach (var order in orders)
            {
                Console.WriteLine($"Order #{order.OrderId}: {order.OrderDetails?.Count ?? 0} items");
            }

            return View(orders);
        }

        // GET: /Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            // Check ownership
            var userId = HttpContext.Session.GetUserId()!.Value;
            if (order.UserId != userId && !HttpContext.Session.IsStaff())
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // GET: /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var cart = await _cartService.GetCartAsync(userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            return View(cart);
        }

        // POST: /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress)  // ← Xóa tham số note
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng";
                return RedirectToAction(nameof(Checkout));
            }

            var userId = HttpContext.Session.GetUserId()!.Value;

            // Get cart total
            var totalAmount = await _cartService.GetCartTotalAsync(userId);

            if (totalAmount <= 0)
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Create order (Xóa note parameter)
            var result = await _orderService.CreateOrderAsync(userId, totalAmount, shippingAddress);

            if (result.success)
            {
                // Clear session cart count
                HttpContext.Session.SetInt32("CartCount", 0);

                TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng: #" + result.orderId;
                return RedirectToAction(nameof(Detail), new { id = result.orderId });
            }

            TempData["Error"] = result.message;
            return RedirectToAction(nameof(Checkout));
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // 1. Kiểm tra đăng nhập
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Lấy ID user hiện tại để đảm bảo bảo mật (chỉ hủy đơn của chính mình)
            var userId = HttpContext.Session.GetUserId();

            // Kiểm tra xem đơn hàng có phải của user này không (Optional nhưng nên làm)
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            // 3. Kiểm tra trạng thái có được phép hủy không (Pending mới được hủy)
            if (order.Status != "Pending")
            {
                TempData["Error"] = "Đơn hàng đã được xử lý, không thể hủy.";
                return RedirectToAction("Detail", new { id });
            }

            // 4. GỌI SERVICE ĐỂ HỦY VÀ CỘNG LẠI KHO
            // Lưu ý: userId ở đây đóng vai trò người thực hiện (null hoặc id user đều được vì logic hủy trong service không check quyền user)
            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, "Cancelled");

            if (success)
            {
                TempData["Success"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Detail", new { id });
        }
    }
}