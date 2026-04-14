using Microsoft.AspNetCore.Mvc;
using ShopMVC.Data;
using ShopMVC.Helpers;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using ShopMVC.ViewModels;

namespace ShopMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService; // <--- THÊM DÒNG NÀY
        private readonly ApplicationDbContext _context;

        public CartController(ICartService cartService, IOrderService orderService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _orderService = orderService; // <--- THÊM DÒNG NÀY
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetUserId()!.Value;

            // 1. Lấy dữ liệu gốc (Model) từ Service
            var cartModel = await _cartService.GetCartAsync(userId);

            // 2. Tạo ViewModel (mà View đang yêu cầu)
            var cartViewModel = new CartViewModel();

            // 3. Thực hiện chuyển đổi (map) dữ liệu
            if (cartModel != null && cartModel.CartDetails.Any())
            {
                // Duyệt qua từng chi tiết giỏ hàng (CartDetail) và chuyển nó
                // thành đối tượng mà CartViewModel yêu cầu (vw_CartDetailsWithProduct)
                foreach (var item in cartModel.CartDetails)
                {
                    // vw_CartDetailsWithProduct là một class, chúng ta có thể "new" nó
                    var cartItemView = new vw_CartDetailsWithProduct
                    {
                        CartDetailId = item.CartDetailId,
                        CartId = item.CartId,
                        ProductId = item.ProductId,
                        ProductName = item.Product.ProductName,
                        CategoryName = item.Product.Category.CategoryName,
                        Image = item.Product.Image,
                        UnitPrice = item.Product.Price, // Giá của sản phẩm
                        Quantity = item.Quantity,
                        LineTotal = item.Product.Price * item.Quantity, // Tính tổng dòng
                        AvailableStock = item.Product.Stock
                    };
                    cartViewModel.CartItems.Add(cartItemView);
                }
            }
            // Các thuộc tính khác như SubTotal, Total sẽ tự động tính toán
            // bên trong CartViewModel.cs

            // 4. Gửi đối tượng ViewModel (đã đúng kiểu) cho View
            return View(cartViewModel);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var result = await _cartService.AddToCartAsync(userId, productId, quantity);

            // Update session cart count
            HttpContext.Session.SetInt32("CartCount", result.cartCount);

            return Json(new
            {
                success = result.success,
                message = result.message,
                cartCount = result.cartCount
            });
        }



        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartDetailId, int quantity)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            // Service trả về (success, message, itemSubtotal, subtotal, ..., cartCount)
            var result = await _cartService.UpdateQuantityAsync(cartDetailId, quantity, userId);

            if (result.success)
            {
                HttpContext.Session.SetInt32("CartCount", result.cartCount);
            }

            // === SỬA ĐỔI BẮT ĐẦU TỪ ĐÂY ===
            // Tính toán lại phí ship và tổng tiền cuối cùng dựa trên subtotal mới
            decimal shippingFee = (result.subtotal > 5000000) ? 0 : 30000;
            decimal finalTotal = result.subtotal + shippingFee;
            // === KẾT THÚC SỬA ĐỔI ===

            return Json(new
            {
                success = result.success,
                message = result.message,
                itemSubtotal = result.itemSubtotal,
                subtotal = result.subtotal,
                shippingFee = shippingFee,     // <-- Gửi phí ship MỚI
                totalAmount = finalTotal,    // <-- Gửi tổng tiền MỚI
                cartCount = result.cartCount
            });
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartDetailId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var result = await _cartService.RemoveFromCartAsync(cartDetailId, userId);

            // Update session cart count
            if (result.success)
            {
                HttpContext.Session.SetInt32("CartCount", result.cartCount);
            }

            return Json(new
            {
                success = result.success,
                message = result.message,
                cartCount = result.cartCount
            });
        }

        // GET: Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get cart using service
                var cart = await _cartService.GetCartAsync(userId.Value);

                if (cart == null || !cart.CartDetails.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index");
                }

                // Get user info
                var user = await _context.Users.FindAsync(userId.Value);

                // Map to view items
                var cartItems = cart.CartDetails.Select(cd => new vw_CartDetailsWithProduct
                {
                    CartDetailId = cd.CartDetailId,
                    ProductId = cd.ProductId,
                    ProductName = cd.Product.ProductName,
                    Image = cd.Product.Image,
                    Price = cd.Product.Price,
                    Quantity = cd.Quantity,
                    LineTotal = cd.Product.Price * cd.Quantity,
                    CategoryName = cd.Product.Category?.CategoryName
                }).ToList();

                // === BƯỚC 1: TÍNH TOÁN PHÍ SHIP ===
                // Tính tổng tạm tính để kiểm tra điều kiện Freeship
                decimal currentSubTotal = cartItems.Sum(x => x.LineTotal ?? 0);

                // Áp dụng logic: > 5 triệu thì Free, ngược lại 30k
                decimal calculatedShippingFee = (currentSubTotal > 5000000) ? 0 : 30000;

                // === BƯỚC 2: KHỞI TẠO MODEL ===
                var model = new CheckoutViewModel
                {
                    FullName = user?.FullName ?? "",
                    Email = user?.Email ?? "",
                    Phone = user?.Phone ?? "",
                    ShippingAddress = user?.Address ?? "",
                    CartItems = cartItems,

                    // Chỉ cần gán ShippingFee, SubTotal và Total sẽ tự động tính trong Class
                    ShippingFee = calculatedShippingFee
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction("Checkout");
            }

            try
            {
                // 1. Lấy thông tin giỏ hàng để tính tổng tiền
                var cart = await _cartService.GetCartAsync(userId.Value);
                if (cart == null || !cart.CartDetails.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index");
                }

                // Tính toán tổng tiền (Backend validation)
                var subtotal = cart.CartDetails.Sum(cd => cd.Product.Price * cd.Quantity);
                var shippingFee = (subtotal > 5000000) ? 0 : 30000;
                var totalAmount = subtotal + shippingFee;

                // 2. GỌI SERVICE ĐỂ TẠO ĐƠN VÀ TRỪ KHO (Thay vì tự add vào _context)
                var (success, message, orderId) = await _orderService.CreateOrderAsync(userId.Value, totalAmount, model.ShippingAddress);

                if (success)
                {
                    // Xóa session đếm giỏ hàng
                    HttpContext.Session.SetInt32("CartCount", 0);

                    TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng: #" + orderId;
                    return RedirectToAction("Detail", "Order", new { id = orderId });
                }
                else
                {
                    // Nếu lỗi (ví dụ: Hết hàng), thông báo lỗi trả về từ Service
                    TempData["Error"] = message;
                    return RedirectToAction("Checkout");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }



        // POST: /Cart/ClearCart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var userId = HttpContext.Session.GetUserId()!.Value;
            var result = await _cartService.ClearCartAsync(userId);

            // Clear session cart count
            if (result.success)
            {
                HttpContext.Session.SetInt32("CartCount", 0);
            }

            return Json(new
            {
                success = result.success,
                message = result.message
            });
        }
    }
}