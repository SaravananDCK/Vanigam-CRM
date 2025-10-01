using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Vendor : LedgerAccount
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerType Type { get; set; } = CustomerType.Company;

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(500)]
        [Url]
        public string? Website { get; set; }

        [StringLength(100)]
        public string? TaxId { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        [StringLength(10)]
        public string? Rating { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaymentTermsDays { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    }
}
