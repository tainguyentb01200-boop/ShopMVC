using Microsoft.AspNetCore.Mvc;
using ShopMVC.Services;
using ShopMVC.Services.Interfaces;

namespace ShopMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Product
        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
        {
            try
            {
                // FIX: Thêm tham số true vào cuối để chỉ lấy sản phẩm IsActive = true
                var model = await _productService.GetProductsPagedAsync(categoryId, search, page, isActive: true);

                return View(model);
            }
            catch (Exception ex)
            {
                // Log lỗi (có thể dùng ILogger thay vì Console.WriteLine trong Production)
                Console.WriteLine($"Error: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Product/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                // FIX: Kiểm tra thêm điều kiện !product.IsActive
                // Nếu sản phẩm không tìm thấy HOẶC đã ngừng bán thì báo lỗi
                if (product == null || !product.IsActive)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: /Product/Category/1
        public async Task<IActionResult> Category(int id, int page = 1)
        {
            try
            {
                // FIX: Thêm tham số true để chỉ lấy sản phẩm đang hoạt động trong danh mục
                var model = await _productService.GetProductsPagedAsync(id, null, page, isActive: true);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Product/Search?term=iphone
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(term);

                // Lọc lại phía client nếu SP Search không hỗ trợ lọc Active
                // (Tốt nhất là nên sửa trong SP Search, nhưng tạm thời có thể lọc ở đây)
                var activeProducts = products.Where(p => p.IsActive).ToList();

                return Json(activeProducts);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}