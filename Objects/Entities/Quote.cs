using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Quote : Voucher
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        public Guid? OpportunityId { get; set; }
        [ForeignKey(nameof(OpportunityId))]
        public Opportunity? Opportunity { get; set; }

        public Guid? JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }

        public override void CalculateTotal()
        {
            SubTotal = Items.Sum(i => (i.UnitPrice * (decimal)i.Quantity));
            TaxAmount = Items.Sum(i => i.TaxAmount);
            DiscountAmount = Items.Sum(i => i.DiscountAmount);
            TotalAmount = Items.Sum(i => i.LineTotal);
        }
        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
