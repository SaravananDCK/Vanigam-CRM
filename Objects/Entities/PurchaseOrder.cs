using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class PurchaseOrder : Voucher
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        public DateTimeOffset? ExpectedDeliveryDate { get; set; }

        [StringLength(100)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        public override void CalculateTotal()
        {
            SubTotal = VoucherLines.OfType<PurchaseOrderItem>().Sum(i => (i.UnitPrice * (decimal)i.Quantity));
            TaxAmount = VoucherLines.OfType<PurchaseOrderItem>().Sum(i => i.TaxAmount);
            DiscountAmount = VoucherLines.OfType<PurchaseOrderItem>().Sum(i => i.DiscountAmount);
            TotalAmount = VoucherLines.OfType<PurchaseOrderItem>().Sum(i => i.LineTotal);
        }
    }

    public enum PurchaseOrderStatus
    {
        Draft,
        Sent,
        Confirmed,
        PartiallyReceived,
        Received,
        Cancelled
    }
}
