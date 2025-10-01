using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NodaTime;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class StockLedgerEntry : BaseClass
    {
        public DateTimeOffset EntryDate { get; set; } = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

        [Required]
        [StringLength(50)]
        public string EntryNumber { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StockMovementType MovementType { get; set; }

        public Guid? InventoryItemId { get; set; }
        [ForeignKey(nameof(InventoryItemId))]
        public InventoryItem? InventoryItem { get; set; }

        public Guid? LocationId { get; set; }
        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityIn { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityOut { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        public Guid? VoucherId { get; set; }
        [ForeignKey(nameof(VoucherId))]
        public Voucher? Voucher { get; set; }

        public Guid? VoucherLineId { get; set; }
        [ForeignKey(nameof(VoucherLineId))]
        public VoucherLine? VoucherLine { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        public DateTimeOffset? ExpiryDate { get; set; }
    }

    public enum StockMovementType
    {
        Purchase,
        Sale,
        Return,
        Adjustment,
        Transfer,
        Opening,
        Consumption
    }
}
