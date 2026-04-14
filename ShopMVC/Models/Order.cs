using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Index("ApprovedBy", Name = "IX_Orders_ApprovedBy")]
[Index("OrderDate", Name = "IX_Orders_OrderDate", AllDescending = true)]
[Index("Status", Name = "IX_Orders_Status")]
[Index("UserId", Name = "IX_Orders_UserId")]
public partial class Order
{
    [Key]
    public int OrderId { get; set; }

    public int UserId { get; set; }

    [Precision(0)]
    public DateTime OrderDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(500)]
    public string ShippingAddress { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    [Precision(0)]
    public DateTime? ApprovedDate { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("OrderApprovedByNavigations")]
    public virtual User? ApprovedByNavigation { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [ForeignKey("UserId")]
    [InverseProperty("OrderUsers")]
    public virtual User User { get; set; } = null!;
}
