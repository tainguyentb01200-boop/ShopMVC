using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Models;
using ShopMVC.Services.Interfaces;
using System.Data;

namespace ShopMVC.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public OrderService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("PhoneShopDB");
        }

        // CREATE ORDER (Đã sửa: Trừ kho ngay khi đặt)
        public async Task<(bool success, string message, int orderId)> CreateOrderAsync(int userId, decimal totalAmount, string shippingAddress)
        {
            // Sử dụng Transaction để đảm bảo tính toàn vẹn: 
            // Nếu tạo đơn lỗi thì KHÔNG trừ kho, nếu trừ kho lỗi thì KHÔNG tạo đơn.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy giỏ hàng và sản phẩm để kiểm tra tồn kho
                var cart = await _context.Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartDetails.Any())
                {
                    return (false, "Giỏ hàng trống", 0);
                }

                // 2. KIỂM TRA VÀ TRỪ TỒN KHO
                foreach (var item in cart.CartDetails)
                {
                    // Kiểm tra tồn kho
                    if (item.Product.Stock < item.Quantity)
                    {
                        // Nếu không đủ hàng -> Rollback (Hủy toàn bộ thao tác) và báo lỗi
                        await transaction.RollbackAsync();
                        return (false, $"Sản phẩm '{item.Product.ProductName}' không đủ hàng (Còn: {item.Product.Stock})", 0);
                    }

                    // TRỪ KHO NGAY LẬP TỨC
                    item.Product.Stock -= item.Quantity;
                }

                // 3. Tạo đơn hàng (Order)
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = "Pending", // Mặc định là Chờ duyệt
                    ShippingAddress = shippingAddress,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy OrderId

                // 4. Tạo chi tiết đơn hàng (OrderDetail)
                foreach (var item in cart.CartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                // 5. Xóa giỏ hàng cũ
                _context.CartDetails.RemoveRange(cart.CartDetails);

                await _context.SaveChangesAsync();

                // 6. Commit Transaction (Xác nhận mọi thay đổi thành công)
                await transaction.CommitAsync();

                return (true, "Đặt hàng thành công", order.OrderId);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi bất kỳ -> Hoàn tác tất cả (bao gồm việc trừ kho)
                await transaction.RollbackAsync();
                return (false, "Lỗi: " + ex.Message, 0);
            }
        }

        // GET ORDER BY ID
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetOrderById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    Order? order = null;
                    var orderDetails = new List<OrderDetail>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read Order
                        if (await reader.ReadAsync())
                        {
                            order = new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                ApprovedBy = reader.IsDBNull(reader.GetOrdinal("ApprovedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy")),
                                ApprovedDate = reader.IsDBNull(reader.GetOrdinal("ApprovedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate")),

                                // ← THÊM PHẦN NÀY: Load User info
                                User = new User
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("UserFullName")) ? "" : reader.GetString(reader.GetOrdinal("UserFullName")),
                                    Email = reader.IsDBNull(reader.GetOrdinal("UserEmail")) ? "" : reader.GetString(reader.GetOrdinal("UserEmail")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("UserPhoneNumber")) ? "" : reader.GetString(reader.GetOrdinal("UserPhoneNumber"))  // ← THÊM DÒNG NÀY
                                }
                            };
                        }

                        // Read Order Details
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                orderDetails.Add(new OrderDetail
                                {
                                    OrderDetailId = reader.GetInt32(reader.GetOrdinal("OrderDetailId")),
                                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Product = new Product
                                    {
                                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                        Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader.GetString(reader.GetOrdinal("Image")),
                                        Category = new Category
                                        {
                                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                                        }
                                    }
                                });
                            }
                        }
                    }

                    if (order != null)
                    {
                        order.OrderDetails = orderDetails;

                        // ← THÊM PHẦN NÀY: Load ApprovedBy user info
                        if (order.ApprovedBy != null)
                        {
                            order.ApprovedByNavigation = await _context.Users
                                .Where(u => u.UserId == order.ApprovedBy)
                                .Select(u => new User { UserId = u.UserId, FullName = u.FullName })
                                .FirstOrDefaultAsync();
                        }
                    }

                    return order;
                }
            }
        }

        // GET ORDERS BY USER
        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, string? status = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetOrdersByUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Status", status as object ?? DBNull.Value);

                    var orders = new List<Order>();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                ApprovedBy = reader.IsDBNull(reader.GetOrdinal("ApprovedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy")),
                                ApprovedDate = reader.IsDBNull(reader.GetOrdinal("ApprovedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate"))
                            });
                        }
                    }

                    return orders;
                }
            }
        }

        public async Task<(bool success, string message)> UpdateOrderStatusAsync(int orderId, string newStatus, int? approvedBy = null)
        {
            try
            {
                // 1. Lấy đơn hàng kèm chi tiết sản phẩm
                var order = await _context.Orders
                    .Include(o => o.OrderDetails) // <--- Bắt buộc phải có dòng này mới lấy được list sản phẩm để cộng lại
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null) return (false, "Đơn hàng không tồn tại");

                // 2. LOGIC HOÀN LẠI KHO (Restock)
                // Điều kiện:
                // - Trạng thái MỚI là: Cancelled (Hủy) hoặc Rejected (Từ chối)
                // - Trạng thái CŨ phải là các trạng thái đang hoạt động (chưa bị hủy trước đó)
                bool isNewStatusCancel = (newStatus == "Cancelled" || newStatus == "Rejected");
                bool isOldStatusActive = (order.Status != "Cancelled" && order.Status != "Rejected");

                if (isNewStatusCancel && isOldStatusActive)
                {
                    foreach (var item in order.OrderDetails)
                    {
                        // Tìm sản phẩm trong kho
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            // === CỘNG LẠI SỐ LƯỢNG VÀO KHO ===
                            product.Stock += item.Quantity;
                        }
                    }
                }

                // 3. Cập nhật trạng thái
                order.Status = newStatus;

                // Cập nhật thông tin người duyệt (nếu có)
                if (approvedBy != null)
                {
                    order.ApprovedBy = approvedBy;
                    order.ApprovedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return (true, "Cập nhật trạng thái thành công");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        // GET ALL ORDERS (Admin)
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // GET ORDERS BY STATUS (Admin)
        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // GET ORDER DETAILS
        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            return await _context.OrderDetails
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }

        // APPROVE ORDER
        public async Task<bool> ApproveOrderAsync(int orderId, int staffId)
        {
            var result = await UpdateOrderStatusAsync(orderId, "Approved", staffId);
            return result.success;
        }

        // REJECT ORDER
        public async Task<bool> RejectOrderAsync(int orderId, int staffId)
        {
            var result = await UpdateOrderStatusAsync(orderId, "Rejected", staffId);
            return result.success;
        }

        // GET USER ORDERS (for admin view)
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await GetOrdersByUserAsync(userId);
        }
    }
}