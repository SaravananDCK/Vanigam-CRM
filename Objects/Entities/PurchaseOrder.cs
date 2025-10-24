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
