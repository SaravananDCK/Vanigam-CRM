using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public abstract class VoucherLine : BaseClass
    {
        [Required]
        public Guid VoucherId { get; set; }
        [ForeignKey(nameof(VoucherId))]
        public Voucher? Voucher { get; set; }

        public int LineNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VoucherLineType LineType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Guid? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public Guid? TaxCodeId { get; set; }

        [ForeignKey(nameof(TaxCodeId))]
        public TaxCode? TaxCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public double Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercent { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercent { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; } = 0;

        [StringLength(100)]
        public string? Unit { get; set; }
        public virtual void CalculateTotal()
        {
            var subtotal = (decimal)(Quantity * (double)UnitPrice);
            LineTotal = subtotal - DiscountAmount + TaxAmount;
        }
    }
}
