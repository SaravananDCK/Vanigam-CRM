using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Represents customer advance payment (overpayment or prepayment)
    /// Can be allocated to future invoices or refunded
    /// </summary>
    public class CustomerAdvance : PaymentApplicationBase
    {
        [StringLength(200)]
        public string? Reason { get; set; }

        /// <summary>
        /// Remaining balance available for future allocations
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount { get; set; }

        /// <summary>
        /// Whether this advance is available for auto-allocation
        /// </summary>
        public bool IsAvailableForAllocation { get; set; } = true;

        /// <summary>
        /// Optional expiry date for the advance
        /// </summary>
        public DateTimeOffset? ExpiryDate { get; set; }
    }
}
