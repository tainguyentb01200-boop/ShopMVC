using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Keyless]
public partial class vw_OrderDetailsWithProduct
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string CustomerName { get; set; } = null!;

    public int ProductId { get; set; }

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(500)]
    public string? Image { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(50)]
    public string? Size { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(29, 2)")]
    public decimal? LineTotal { get; set; }
}
