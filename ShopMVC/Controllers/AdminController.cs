using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Services.Interfaces;
using ShopMVC.ViewModels;
using ShopMVC.Models;
using ShopMVC.Helpers;

namespace ShopMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            IAdminService adminService,
            IProductService productService,
            IOrderService orderService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _productService = productService;
            _orderService = orderService;
            _context = context;
            _environment = environment;
        }

        private bool CheckAdminAccess()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return false;
            }

            if (!HttpContext.Session.IsStaff())
            {
                TempData["Error"] = "Bạn không có quyền truy cập";
                return false;
            }

            return true;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var stats = await _adminService.GetDashboardStatsAsync();
            var recentOrders = (await _orderService.GetAllOrdersAsync()).Take(10).ToList();
            var topProducts = await _adminService.GetTopProductsAsync(5);

            var viewModel = new DashboardViewModel
            {
                Stats = stats,
                RecentOrders = recentOrders,
                TopProducts = topProducts
            };

            return View(viewModel);
        }

        // =============================================
        // QUẢN LÝ SẢN PHẨM
        // =============================================

        // GET: /Admin/Products
        public async Task<IActionResult> Products(int page = 1, int? categoryId = null, string? search = null, bool? isActive = null)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _productService.GetProductsPagedAsync(categoryId, search, page, isActive);

            return View(model);
        }

        // GET: /Admin/CreateProduct
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product model, IFormFile? imageFile)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Category");
            ModelState.Remove("Image");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            try
            {
                if (imageFile != null)
                {
                    var uploadResult = await ImageHelper.UploadProductImageAsync(imageFile, _environment);
                    if (uploadResult.success)
                    {
                        model.Image = uploadResult.fileName;
                    }
                    else
                    {
                        TempData["Error"] = uploadResult.error;
                        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                        return View(model);
                    }
                }

                var productId = await _productService.CreateProductAsync(model);

                if (productId > 0)
                {
                    TempData["Success"] = "Thêm sản phẩm thành công";
                    return RedirectToAction(nameof(Products));
                }

                TempData["Error"] = "Không thể thêm sản phẩm";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }
        }

        // GET: /Admin/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product model, IFormFile? imageFile)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.ProductId)
            {
                return NotFound();
            }

            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            try
            {
                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(model.Image))
                    {
                        ImageHelper.DeleteProductImage(model.Image, _environment);
                    }

                    var uploadResult = await ImageHelper.UploadProductImageAsync(imageFile, _environment);
                    if (uploadResult.success)
                    {
                        model.Image = uploadResult.fileName;
                    }
                }

                var result = await _productService.UpdateProductAsync(model);

                if (result)
                {
                    TempData["Success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction(nameof(Products));
                }

                TempData["Error"] = "Không thể cập nhật sản phẩm";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _productService.DeleteProductAsync(id);

            if (result)
            {
                TempData["Success"] = "Xóa sản phẩm thành công";
            }
            else
            {
                TempData["Error"] = "Không thể xóa sản phẩm";
            }

            return RedirectToAction(nameof(Products));
        }

        // =============================================
        // QUẢN LÝ ĐƠN HÀNG
        // =============================================

        // GET: /Admin/Orders
        public async Task<IActionResult> Orders(string? status = null)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = string.IsNullOrEmpty(status)
                ? (await _orderService.GetAllOrdersAsync()).ToList()
                : (await _orderService.GetOrdersByStatusAsync(status)).ToList();

            var viewModel = new AdminOrderViewModel
            {
                Orders = orders,
                StatusFilter = status
            };

            return View(viewModel);
        }

        // GET: /Admin/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Orders));
            }

            var orderDetails = (await _orderService.GetOrderDetailsAsync(id)).ToList();

            var viewModel = new OrderViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            };
            ViewBag.NextStatuses = OrderStatusHelper.GetAvailableStatuses(order.Status);
            return View(viewModel);
        }

        // POST: /Admin/ApproveOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var staffId = HttpContext.Session.GetUserId()!.Value;
            var result = await _orderService.ApproveOrderAsync(id, staffId);

            if (result)
            {
                TempData["Success"] = "Đã duyệt đơn hàng";
            }
            else
            {
                TempData["Error"] = "Không thể duyệt đơn hàng";
            }

            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }
            // 1. Lấy đơn hàng hiện tại để kiểm tra trạng thái cũ
            var currentOrder = await _orderService.GetOrderByIdAsync(orderId);
            if (currentOrder == null) return NotFound();

            // 2. Kiểm tra xem trạng thái mới có hợp lệ so với trạng thái cũ không
            var allowedStatuses = OrderStatusHelper.GetAvailableStatuses(currentOrder.Status);

            // Nếu trạng thái mới không nằm trong danh sách cho phép -> Chặn ngay
            if (!allowedStatuses.ContainsKey(status))
            {
                TempData["Error"] = $"Không thể chuyển từ '{currentOrder.Status}' sang '{status}' theo quy trình.";
                return RedirectToAction(nameof(OrderDetail), new { id = orderId });
            }

            var staffId = HttpContext.Session.GetUserId()!.Value;
            var (success, message) = await _orderService.UpdateOrderStatusAsync(orderId, status, staffId);

            if (success)
            {
                TempData["Success"] = "Đã cập nhật trạng thái đơn hàng";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật trạng thái";
            }


            return RedirectToAction(nameof(OrderDetail), new { id = orderId });
        }

        // =============================================
        // QUẢN LÝ NGƯỜI DÙNG (ADMIN ONLY)
        // =============================================

        // GET: /Admin/Users
        public async Task<IActionResult> Users(string? role = null, string? search = null)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Chỉ Admin mới có quyền quản lý người dùng";
                return RedirectToAction(nameof(Index));
            }

            var users = await _adminService.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    u.FullName.ToLower().Contains(search) ||
                    (u.Email != null && u.Email.ToLower().Contains(search))
                ).ToList();
            }

            var viewModel = new AdminUserViewModel
            {
                Users = users,
                RoleFilter = role,
                SearchTerm = search
            };

            return View(viewModel);
        }

        // GET: /Admin/UserDetail/5
        public async Task<IActionResult> UserDetail(int id)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Chỉ Admin mới có quyền xem chi tiết người dùng";
                return RedirectToAction(nameof(Index));
            }

            var user = await _adminService.GetUserDetailAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng";
                return RedirectToAction(nameof(Users));
            }

            var orders = await _orderService.GetUserOrdersAsync(id);
            ViewBag.Orders = orders;

            return View(user);
        }

        // POST: /Admin/DeactivateUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Chỉ Admin mới có quyền này";
                return RedirectToAction(nameof(Index));
            }

            var result = await _adminService.DeactivateUserAsync(id);

            if (result)
            {
                TempData["Success"] = "Đã vô hiệu hóa tài khoản";
            }
            else
            {
                TempData["Error"] = "Không thể vô hiệu hóa tài khoản";
            }

            return RedirectToAction(nameof(UserDetail), new { id });
        }

        // POST: /Admin/ActivateUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int id)
        {
            if (!HttpContext.Session.IsAdmin())
            {
                TempData["Error"] = "Chỉ Admin mới có quyền này";
                return RedirectToAction(nameof(Index));
            }

            var result = await _adminService.ActivateUserAsync(id);

            if (result)
            {
                TempData["Success"] = "Đã kích hoạt tài khoản";
            }
            else
            {
                TempData["Error"] = "Không thể kích hoạt tài khoản";
            }

            return RedirectToAction(nameof(UserDetail), new { id });
        }

        // =============================================
        // BÁO CÁO & THỐNG KÊ
        // =============================================

        // GET: /Admin/SalesReport
        public async Task<IActionResult> SalesReport(DateTime? fromDate, DateTime? toDate)
        {
            // Set default dates
            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to = toDate ?? DateTime.Today;

            // Ensure from <= to
            if (from > to)
            {
                var temp = from;
                from = to;
                to = temp;
            }

            // CHỈ TÍNH ĐỢN HÀNG HOÀN THÀNH
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to.AddDays(1))
                .Where(o => o.Status == "Completed") // ← CHỈ LẤY ĐƠN HOÀN THÀNH
                .ToListAsync();

            // Lấy tất cả đơn để thống kê trạng thái
            var allOrders = await _context.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to.AddDays(1))
                .ToListAsync();

            var report = new SalesReport
            {
                FromDate = from,
                ToDate = to,
                TotalOrders = allOrders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount ?? 0), // Chỉ tính đơn Completed
                CompletedOrders = allOrders.Count(o => o.Status == "Completed"),
                PendingOrders = allOrders.Count(o => o.Status == "Pending"),
                CancelledOrders = allOrders.Count(o => o.Status == "Cancelled"),
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

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");

            return View(report);
        }

        // GET: /Admin/TopProducts
        public async Task<IActionResult> TopProducts()
        {
            if (!HttpContext.Session.IsStaff())
            {
                TempData["Error"] = "Bạn không có quyền truy cập";
                return RedirectToAction(nameof(Index));
            }

            var topProducts = await _adminService.GetTopProductsAsync(20);
            return View(topProducts);
        }

        // GET: /Admin/CategoryStats
        public async Task<IActionResult> CategoryStats()
        {
            if (!HttpContext.Session.IsStaff())
            {
                TempData["Error"] = "Bạn không có quyền truy cập";
                return RedirectToAction(nameof(Index));
            }

            var categoryStats = await _adminService.GetCategoryStatsAsync();
            return View(categoryStats);
        }
    }
}