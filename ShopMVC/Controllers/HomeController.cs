using Microsoft.AspNetCore.Mvc;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using System.Diagnostics;
using WebApplication1.Models;

namespace ShopMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ILogger<HomeController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Trang chủ - Hiển thị sản phẩm nổi bật
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy 8 sản phẩm mới nhất
                var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage");
                return View(new List<Product>());
            }
        }

        /// <summary>
        /// Trang giới thiệu
        /// </summary>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Trang liên hệ
        /// </summary>
        public IActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Trang lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}