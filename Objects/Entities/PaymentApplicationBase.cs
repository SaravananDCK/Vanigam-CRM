using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Base class for payment applications - supports both invoice allocations and customer advances
    /// Uses Table-Per-Hierarchy (TPH) inheritance with discriminator column
    /// </summary>
    public abstract class PaymentApplicationBase : BaseClass
    {
        [Required]
        public Guid PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTimeOffset AppliedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
