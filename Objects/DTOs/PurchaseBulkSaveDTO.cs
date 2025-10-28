using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.DTOs;

public class PurchaseOrderBulkSaveDTO
{
    public Guid? Oid { get; set; }
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public Guid? VendorId { get; set; }
    public decimal DiscountAmount { get; set; }
    public double DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string ShippingAddress { get; set; }
    public string ContactPerson { get; set; }
    public string Reference { get; set; }
    public List<PurchaseOrderItemDTO> Items { get; set; } = new List<PurchaseOrderItemDTO>();
}

public class PurchaseOrderItemDTO
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

    // For UI display purposes
    public string? InventoryItemName { get; set; }

    // Indicates if this item should be deleted
    public bool IsDeleted { get; set; } = false;

    // Indicates if this is a new item
    public bool IsNew => !Oid.HasValue;
}