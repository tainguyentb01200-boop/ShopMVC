using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Keyless]
public partial class vw_ProductsWithCategory
{
    public int ProductId { get; set; }

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

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
}
