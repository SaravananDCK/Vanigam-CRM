using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NodaTime;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    // Base class for all vouchers using Table-Per-Hierarchy (TPH) inheritance
    [JsonDerivedType(typeof(Invoice), "#Vanigam.CRM.Objects.Entities.Invoice")]
    [JsonDerivedType(typeof(Job), "#Vanigam.CRM.Objects.Entities.Job")]
    [JsonDerivedType(typeof(Payment), "#Vanigam.CRM.Objects.Entities.Payment")]
    [JsonDerivedType(typeof(PurchaseInvoice), "#Vanigam.CRM.Objects.Entities.PurchaseInvoice")]
    [JsonDerivedType(typeof(PurchaseOrder), "#Vanigam.CRM.Objects.Entities.PurchaseOrder")]
    [JsonDerivedType(typeof(Quote), "#Vanigam.CRM.Objects.Entities.Quote")]
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor, TypeDiscriminatorPropertyName = "@odata.type")]
    public abstract class Voucher : BaseClass
    {
        [Required]
        [StringLength(50)]
        public string Number { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VoucherType VoucherType { get; set; }

        public DateTimeOffset VoucherDate { get; set; } = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

        public DateTimeOffset? DueDate { get; set; }

        [StringLength(500)]
        public string? Reference { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DiscountType DiscountType { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;
        
        [DisplayName("Discount (%)")]
        [Range(0, 100)]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public double DiscountPercent { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        public Guid? PartyId { get; set; }
        [ForeignKey(nameof(PartyId))]
        public LedgerAccount? Party { get; set; }

        public virtual void CalculateTotal()
        {
           
        }
        // Navigation properties
        public ICollection<VoucherLine> VoucherLines { get; set; } = new List<VoucherLine>();
        public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}
