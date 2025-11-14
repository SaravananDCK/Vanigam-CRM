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

    public Guid? PartyId { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal CGSTAmount { get; set; } = 0;

    public decimal SGSTAmount { get; set; } = 0;

    public decimal IGSTAmount { get; set; } = 0;

    public decimal CessAmount { get; set; } = 0;
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; }

    public DateTimeOffset VoucherDate { get; set; } = DateTimeOffset.UtcNow;

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

public class PaymentDTO
{
    public Guid? Oid { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public PaymentStatus? Status { get; set; }

    public DateTimeOffset? PaidAt { get; set; }

    [StringLength(100)]
    public string? ProviderReference { get; set; }

    // Indicates if this payment should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new payment
    public bool IsNew => !Oid.HasValue;
}

public class PaymentBulkSaveDTO
{
    public Guid? Oid { get; set; }

    public Guid? PartyId { get; set; }

    [Required]
    public decimal PaymentAmount { get; set; }

    [Required]
    public DateTimeOffset VoucherDate { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }

    public Guid? BankAccountId { get; set; }

    public PaymentStatus? Status { get; set; }

    public decimal AllocatedAmount { get; set; }

    public decimal UnallocatedAmount { get; set; }

    public List<PaymentAllocationDTO> Allocations { get; set; } = new List<PaymentAllocationDTO>();
}

public class PaymentAllocationDTO
{
    public Guid? Oid { get; set; }

    [Required]
    public Guid InvoiceId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    // Additional properties for UI display
    public DateTimeOffset InvoiceDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public bool IsSelected { get; set; }

    // Indicates if this allocation should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new allocation
    public bool IsNew => !Oid.HasValue;
}