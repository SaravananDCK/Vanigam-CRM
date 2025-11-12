using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs;

public class JobBulkSaveDTO
{
    public Guid? Oid { get; set; }

    [Required]
    [StringLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;
    public Priority Priority { get; set; } = Priority.Normal;

    public Guid? PartyId { get; set; }
    public Guid? ContactId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal CGSTAmount { get; set; } = 0;
    public decimal SGSTAmount { get; set; } = 0;
    public decimal IGSTAmount { get; set; } = 0;
    public decimal CessAmount { get; set; } = 0;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }

    public DateTimeOffset VoucherDate { get; set; } = DateTimeOffset.UtcNow;

    public List<MaterialUsageDTO> Materials { get; set; } = new List<MaterialUsageDTO>();
}

public class MaterialUsageDTO
{
    public Guid? Oid { get; set; }

    public Guid? InventoryItemId { get; set; }

    [Required]
    public double Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal? TaxAmount { get; set; }
    public Guid? TaxCodeId { get; set; }

    // GST component rates from TaxCode (for calculation purposes)
    public double CGSTRate { get; set; } = 0;
    public double SGSTRate { get; set; } = 0;
    public double IGSTRate { get; set; } = 0;
    public double CessRate { get; set; } = 0;
    public decimal ChargedAmount { get; set; }
    public decimal WaivedAmount { get; set; }
    public decimal Total => (decimal)Quantity * UnitPrice;
    public decimal TotalIncTax => Total + (TaxAmount ?? 0) - DiscountAmount;

    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}
