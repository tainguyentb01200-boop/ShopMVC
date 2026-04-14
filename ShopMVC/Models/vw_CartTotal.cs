using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Keyless]
public partial class vw_CartTotal
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [Column(TypeName = "decimal(38, 2)")]
    public decimal TotalAmount { get; set; }

    public int? TotalItems { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedDate { get; set; }
}
