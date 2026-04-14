using ShopMVC.Models;

namespace ShopMVC.ViewModels
{
    public class CartViewModel
    {
        public List<vw_CartDetailsWithProduct> CartItems { get; set; } = new List<vw_CartDetailsWithProduct>();

        public decimal SubTotal => CartItems.Sum(item => item.LineTotal ?? 0);

        public decimal ShippingFee
        {
            get
            {
                // Nếu Tạm tính > 5 triệu, phí ship là 0
                if (this.SubTotal > 5000000)
                {
                    return 0;
                }
                // Ngược lại, phí ship là 30.000đ
                return 30000;
            }
        }

        public decimal Total => SubTotal + ShippingFee;

        public int TotalItems => CartItems.Sum(item => item.Quantity); // Fixed: Removed null-coalescing operator as Quantity is non-nullable

        public bool IsEmpty => !CartItems.Any();
    }

    public class CartItemUpdateModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddToCartModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
