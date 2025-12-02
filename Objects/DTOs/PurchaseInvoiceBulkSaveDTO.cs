using System.ComponentModel.DataAnnotations;
using NodaTime;
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

    [StringLength(100)]
    public string? VendorInvoiceNumber { get; set; }

    public DateTimeOffset? ReceivedDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal CGSTAmount { get; set; } = 0;
    public decimal SGSTAmount { get; set; } = 0;
    public decimal IGSTAmount { get; set; } = 0;
    public decimal CessAmount { get; set; } = 0;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }

    public DateTimeOffset VoucherDate { get; set; } = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
    public DateTimeOffset? DueDate { get; set; }

    public Guid? PurchaseOrderId { get; set; }

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

    // GST component rates from TaxCode (for calculation purposes)
    public double CGSTRate { get; set; } = 0;
    public double SGSTRate { get; set; } = 0;
    public double IGSTRate { get; set; } = 0;
    public double CessRate { get; set; } = 0;

    public decimal Total => (decimal)Quantity * UnitPrice;
    public decimal TotalIncTax => Total + (TaxAmount ?? 0) - DiscountAmount;

    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}
