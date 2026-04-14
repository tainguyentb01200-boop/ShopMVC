using ShopMVC.Models;
using ShopMVC.ViewModels;

namespace ShopMVC.Services.Interfaces
{
    public interface IProductService
    {
        // Get products with pagination
        Task<ProductListViewModel> GetProductsPagedAsync(int? categoryId, string? search, int page = 1, bool? isActive = null);

        // Get single product
        Task<Product?> GetProductByIdAsync(int id);

        // Search
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);

        // Get all products
        Task<IEnumerable<Product>> GetAllProductsAsync();

        // Get featured products
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);

        // Get by category
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        // CRUD operations (Admin)
        Task<int> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);

        // Check stock
        Task<bool> CheckStockAsync(int productId, int quantity);
    }
}