using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class PurchaseInvoice : Voucher
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;

        public Guid? PurchaseOrderId { get; set; }
        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrder? PurchaseOrder { get; set; }

        [StringLength(100)]
        public string? VendorInvoiceNumber { get; set; }

        public DateTimeOffset? ReceivedDate { get; set; }
        
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public enum PurchaseInvoiceStatus
    {
        Draft,
        Posted,
        Received,
        Verified,
        Paid,
        [Display(Description = "Partially-Paid")]
        PartiallyPaid,
        Overdue,
        Cancelled
    }
}
