using System.ComponentModel.DataAnnotations;
using ShopMVC.Models;

namespace ShopMVC.ViewModels
{
    public class CheckoutViewModel
    {
        // Thông tin giỏ hàng
        public List<vw_CartDetailsWithProduct> CartItems { get; set; } = new List<vw_CartDetailsWithProduct>();
        
        public decimal SubTotal => CartItems.Sum(item => item.LineTotal ?? 0);
        public decimal ShippingFee { get; set; } = 30000;
        public decimal Total => SubTotal + ShippingFee;
        
        // Thông tin người dùng
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // Địa chỉ giao hàng
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ không quá 500 ký tự")]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Display(Name = "Ghi chú đơn hàng")]
        [StringLength(1000, ErrorMessage = "Ghi chú không quá 1000 ký tự")]
        public string? Notes { get; set; }
        
        // Phương thức thanh toán
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD"; // Cash on Delivery
    }
}
