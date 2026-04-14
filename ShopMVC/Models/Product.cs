using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Index("CategoryId", Name = "IX_Products_CategoryId")]
[Index("IsActive", Name = "IX_Products_IsActive")]
[Index("ProductName", Name = "IX_Products_ProductName")]
public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Image { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(50)]
    public string? Size { get; set; }

    public string? Description { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
