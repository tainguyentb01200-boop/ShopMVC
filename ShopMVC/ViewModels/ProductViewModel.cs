using ShopMVC.Models;

namespace ShopMVC.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; } = new Product();
        public string CategoryName { get; set; } = string.Empty;
        public bool IsInStock => Product.Stock > 0;
        public bool IsLowStock => Product.Stock > 0 && Product.Stock <= 10;
        
        // Cho trang danh sách
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }
        
        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
    }

}
