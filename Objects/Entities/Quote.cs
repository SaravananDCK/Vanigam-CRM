using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Quote : BaseClass
    {
        [Required]
        [StringLength(50)]
        public string Number { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        public Guid? OpportunityId { get; set; }
        [ForeignKey(nameof(OpportunityId))]
        public Opportunity? Opportunity { get; set; }

        public Guid? CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public Guid? JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }
        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }
}
