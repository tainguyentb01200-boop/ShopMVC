using ShopMVC.Models;

namespace ShopMVC.Services.Interfaces
{
    public interface ICartService
    {
        // Get cart
        Task<Cart?> GetCartAsync(int userId);

        // Add to cart
        Task<(bool success, string message, int cartCount)> AddToCartAsync(int userId, int productId, int quantity);

        // Update quantity
        Task<(bool success, string message, decimal itemSubtotal, decimal subtotal, decimal totalAmount, int cartCount)> UpdateQuantityAsync(int cartDetailId, int quantity, int userId);

        // Remove from cart
        Task<(bool success, string message, int cartCount)> RemoveFromCartAsync(int cartDetailId, int userId);

        // Clear cart
        Task<(bool success, string message)> ClearCartAsync(int userId);

        // Get cart count
        Task<int> GetCartCountAsync(int userId);

        // Get cart item count (THÊM METHOD NÀY)
        Task<int> GetCartItemCountAsync(int userId);

        // Get cart total
        Task<decimal> GetCartTotalAsync(int userId);
    }
}