using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using System.Data;

namespace ShopMVC.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public CartService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("PhoneShopDB");
        }

        // GET CART
        public async Task<Cart?> GetCartAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetCart", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    Cart? cart = null;
                    var cartDetails = new List<CartDetail>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read Cart
                        if (await reader.ReadAsync())
                        {
                            cart = new Cart
                            {
                                CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
                            };
                        }

                        // Read Cart Details
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                cartDetails.Add(new CartDetail
                                {
                                    CartDetailId = reader.GetInt32(reader.GetOrdinal("CartDetailId")),
                                    CartId = reader.GetInt32(reader.GetOrdinal("CartId")),
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Product = new Product
                                    {
                                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                        Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                                        Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                        Category = new Category
                                        {
                                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                                        }
                                    }
                                });
                            }
                        }
                    }

                    if (cart != null)
                    {
                        cart.CartDetails = cartDetails;
                    }

                    return cart;
                }
            }
        }

        // ADD TO CART
        public async Task<(bool success, string message, int cartCount)> AddToCartAsync(int userId, int productId, int quantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_AddToCart", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                            var message = reader.GetString(reader.GetOrdinal("Message"));
                            var cartCount = success ? reader.GetInt32(reader.GetOrdinal("CartCount")) : 0;

                            return (success, message, cartCount);
                        }
                    }

                    return (false, "Có lỗi xảy ra", 0);
                }
            }
        }

        // UPDATE QUANTITY
        public async Task<(bool success, string message, decimal itemSubtotal, decimal subtotal, decimal totalAmount, int cartCount)> UpdateQuantityAsync(int cartDetailId, int quantity, int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_UpdateCartQuantity", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CartDetailId", cartDetailId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                            var message = reader.GetString(reader.GetOrdinal("Message"));

                            if (success)
                            {
                                var itemSubtotal = reader.GetDecimal(reader.GetOrdinal("ItemSubtotal"));
                                var subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal"));
                                var totalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                                var cartCount = reader.GetInt32(reader.GetOrdinal("CartCount"));

                                return (true, message, itemSubtotal, subtotal, totalAmount, cartCount);
                            }

                            return (false, message, 0, 0, 0, 0);
                        }
                    }

                    return (false, "Có lỗi xảy ra", 0, 0, 0, 0);
                }
            }
        }

        // REMOVE FROM CART
        public async Task<(bool success, string message, int cartCount)> RemoveFromCartAsync(int cartDetailId, int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_RemoveFromCart", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CartDetailId", cartDetailId);
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                            var message = reader.GetString(reader.GetOrdinal("Message"));
                            var cartCount = success ? reader.GetInt32(reader.GetOrdinal("CartCount")) : 0;

                            return (success, message, cartCount);
                        }
                    }

                    return (false, "Có lỗi xảy ra", 0);
                }
            }
        }

        // CLEAR CART
        public async Task<(bool success, string message)> ClearCartAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ClearCart", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                            var message = reader.GetString(reader.GetOrdinal("Message"));

                            return (success, message);
                        }
                    }

                    return (false, "Có lỗi xảy ra");
                }
            }
        }

        // GET CART COUNT
        public async Task<int> GetCartCountAsync(int userId)
        {
            var cart = await GetCartAsync(userId);
            return cart?.CartDetails.Sum(cd => cd.Quantity) ?? 0;
        }

        // GET CART TOTAL
        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            var cart = await GetCartAsync(userId);
            return cart?.CartDetails.Sum(cd => cd.Product.Price * cd.Quantity) ?? 0;
        }
        public async Task<int> GetCartItemCountAsync(int userId)
        {
            return await GetCartCountAsync(userId);
        }
    }
}