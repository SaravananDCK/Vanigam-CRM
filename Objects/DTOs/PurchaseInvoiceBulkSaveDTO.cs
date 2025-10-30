using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs;
public class PurchaseInvoiceBulkSaveDTO
{
    public Guid? Oid { get; set; }

    [Required]
    [StringLength(50)]
    public string Number { get; set; } = string.Empty;

    public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;

    public Guid? PartyId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public double DiscountPercentage { get; set; }

    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public DateTimeOffset VoucherDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ReceivedDate { get; set; }

    public List<PurchaseInvoiceItemDTO> Items { get; set; } = new List<PurchaseInvoiceItemDTO>();
}

public class PurchaseInvoiceItemDTO
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
    public decimal Total => (decimal)Quantity * UnitPrice;
    public decimal TotalIncTax => Total + (TaxAmount ?? 0) - DiscountAmount;
    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}
