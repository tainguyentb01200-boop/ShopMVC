using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using ShopMVC.ViewModels;
using System.Data;

namespace ShopMVC.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ProductService(ApplicationDbContext context, IConfiguration configuration)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _context = context;
#pragma warning disable CS8601 // Possible null reference assignment.
            _connectionString = configuration.GetConnectionString("PhoneShopDB");
        }

        // GET PRODUCTS WITH PAGINATION (Stored Procedure)
        public async Task<ProductListViewModel> GetProductsPagedAsync(int? categoryId, string? search, int page = 1, bool? isActive = null)
        {
            const int pageSize = 12;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CategoryId", categoryId as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SearchTerm", search as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", isActive as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Page", page);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    var products = new List<Product>();
                    int totalCount = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read products
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? null : reader.GetString(reader.GetOrdinal("Size")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                                }
                            });
                        }

                        // Read total count
                        if (await reader.NextResultAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                totalCount = reader.GetInt32(0);
                            }
                        }
                    }

                    // Get categories
                    var categories = await GetCategoriesAsync();
                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    return new ProductListViewModel
                    {
                        Products = products,
                        Categories = categories.ToList(),
                        SelectedCategoryId = categoryId,
                        SearchTerm = search,
                        Page = page,
                        PageSize = pageSize,
                        TotalProducts = totalCount,
                        TotalPageCount = totalPages
                    };
                }
            }
        }

        // GET PRODUCT BY ID
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetProductById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
                                Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? null : reader.GetString(reader.GetOrdinal("Size")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Category = new Category
                                {
                                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                                }
                            };
                        }
                    }
                }
            }

            return null;
        }

        // SEARCH PRODUCTS
        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_SearchProducts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    var products = new List<Product>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                Category = new Category
                                {
                                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                                }
                            });
                        }
                    }

                    return products;
                }
            }
        }

        // GET ALL PRODUCTS (cho Home page)
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();
        }

        // GET FEATURED PRODUCTS
        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.Products
                .Where(p => p.Stock > 0 && p.IsActive)
                .OrderByDescending(p => p.ProductId)
                .Take(count)
                .Include(p => p.Category)
                .ToListAsync();
        }

        // CREATE PRODUCT (Admin)
        public async Task<int> CreateProductAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_CreateProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@Description", product.Description as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);
                    command.Parameters.AddWithValue("@Image", product.Image as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Color", product.Color as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Size", product.Size as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsActive", product.IsActive);

                    // Add OUTPUT parameter
                    var outputParam = new SqlParameter("@NewProductId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    // Execute and read result
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Đọc từ SELECT statement trong stored procedure
                            return reader.GetInt32(reader.GetOrdinal("ProductId"));
                        }
                    }

                    // Nếu không đọc được từ reader, lấy từ OUTPUT parameter
                    return outputParam.Value != DBNull.Value ? (int)outputParam.Value : -1;
                }
            }
        }

        // UPDATE PRODUCT (Admin)
        public async Task<bool> UpdateProductAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_UpdateProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ProductId", product.ProductId);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@Description", product.Description as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Stock", product.Stock);
                    command.Parameters.AddWithValue("@Image", product.Image as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Color", product.Color as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Size", product.Size as object ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@IsActive", product.IsActive);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                        }
                    }

                    return false;
                }
            }
        }

        // DELETE PRODUCT (Admin)
        public async Task<bool> DeleteProductAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_DeleteProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                        }
                    }

                    return false;
                }
            }
        }

        // HELPER: GET CATEGORIES
        private async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetCategories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IsActiveOnly", true);

                    var categories = new List<Category>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
                            });
                        }
                    }

                    return categories;
                }
            }
        }
        // GET PRODUCTS BY CATEGORY
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();
        }

        // CHECK STOCK
        public async Task<bool> CheckStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.Stock >= quantity;
        }
    }
}