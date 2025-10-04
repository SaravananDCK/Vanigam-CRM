using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Represents allocation of a payment to a specific invoice
    /// Supports partial payments across multiple invoices
    /// </summary>
    public class PaymentAllocation : PaymentApplicationBase
    {
        [Required]
        public Guid InvoiceId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; } = null!;

        /// <summary>
        /// Snapshot of invoice balance before this allocation
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceBalanceBefore { get; set; }

        /// <summary>
        /// Snapshot of invoice balance after this allocation
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceBalanceAfter { get; set; }
    }
}
