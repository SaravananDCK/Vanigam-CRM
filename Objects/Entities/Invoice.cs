using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Invoice : BaseClass
    {
        [Required]
        [StringLength(50)]
        public string Number { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public Guid? JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
