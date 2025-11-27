using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs;

public class QuoteBulkSaveDTO
{
    public Guid? Oid { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

    public Guid? OpportunityId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? JobId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CGSTAmount { get; set; } = 0;
    public decimal SGSTAmount { get; set; } = 0;
    public decimal IGSTAmount { get; set; } = 0;
    public decimal CessAmount { get; set; } = 0;
    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public List<QuoteItemDTO> Items { get; set; } = new List<QuoteItemDTO>();
}

public class QuoteItemDTO
{
    public Guid? Oid { get; set; }

    public Guid? InventoryItemId { get; set; }

    [Required]
    public double Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public Guid? TaxCodeId { get; set; }
    public decimal? TaxAmount { get; set; }

    public decimal Total => (decimal)(Quantity * (double)UnitPrice);
    public decimal TotalIncTax => Total + (TaxAmount ?? 0) - DiscountAmount;
    public double CGSTRate { get; set; } = 0;
    public double SGSTRate { get; set; } = 0;
    public double IGSTRate { get; set; } = 0;
    public double CessRate { get; set; } = 0;

    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}