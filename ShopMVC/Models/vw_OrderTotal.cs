using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShopMVC.Models;

[Keyless]
public partial class vw_OrderTotal
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string CustomerName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [Precision(0)]
    public DateTime OrderDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(500)]
    public string ShippingAddress { get; set; } = null!;

    [StringLength(100)]
    public string? ApprovedByStaff { get; set; }

    [Precision(0)]
    public DateTime? ApprovedDate { get; set; }

    public int? TotalItems { get; set; }
}
