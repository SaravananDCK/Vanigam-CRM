using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs;

public class InvoiceBulkSaveDTO
{
    public Guid? Oid { get; set; }

    [Required]
    [StringLength(50)]
    public string Number { get; set; } = string.Empty;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public Guid? JobId { get; set; }

    public decimal TotalAmount { get; set; }

    public List<InvoiceItemDTO> Items { get; set; } = new List<InvoiceItemDTO>();
}

public class InvoiceItemDTO
{
    public Guid? Oid { get; set; }

    public Guid? InventoryItemId { get; set; }

    [Required]
    public double Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total => (decimal)Quantity * UnitPrice;

    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}

public class PaymentDTO
{
    public Guid? Oid { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTimeOffset? PaidAt { get; set; }

    [StringLength(100)]
    public string? ProviderReference { get; set; }

    // Indicates if this payment should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new payment
    public bool IsNew => !Oid.HasValue;
}