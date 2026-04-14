using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Index("CartId", Name = "IX_CartDetails_CartId")]
[Index("ProductId", Name = "IX_CartDetails_ProductId")]
[Index("CartId", "ProductId", Name = "UQ_CartDetails_CartProduct", IsUnique = true)]
public partial class CartDetail
{
    [Key]
    public int CartDetailId { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [ForeignKey("CartId")]
    [InverseProperty("CartDetails")]
    public virtual Cart Cart { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("CartDetails")]
    public virtual Product Product { get; set; } = null!;
}
