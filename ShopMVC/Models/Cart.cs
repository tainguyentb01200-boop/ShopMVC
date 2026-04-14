using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Index("UserId", Name = "IX_Carts_UserId")]
public partial class Cart
{
    [Key]
    public int CartId { get; set; }

    public int UserId { get; set; }

    [Precision(0)]
    public DateTime CreatedDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [InverseProperty("Cart")]
    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    [ForeignKey("UserId")]
    [InverseProperty("Carts")]
    public virtual User User { get; set; } = null!;
}
