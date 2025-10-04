using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Represents a payment received from a customer
    /// Supports both single invoice payments and bulk payments across multiple invoices
    /// </summary>
    public class Payment : BaseClass
    {
        /// <summary>
        /// Customer making the payment (required for bulk payments)
        /// </summary>
        [Required]
        public Guid? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        /// <summary>
        /// Total payment amount received
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Amount allocated to invoices
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Amount not yet allocated (available for future allocation or as advance)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnallocatedAmount { get; set; }

        /// <summary>
        /// Date when payment was received
        /// </summary>
        [Required]
        public DateTimeOffset PaymentDate { get; set; }

        /// <summary>
        /// Payment method used (Cash, Bank Transfer, Cheque, Card, UPI, etc.)
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Reference number (Cheque No, UTR ID, Transaction ID, etc.)
        /// </summary>
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Bank account where payment was received
        /// </summary>
        public Guid? BankAccountId { get; set; }

        [ForeignKey(nameof(BankAccountId))]
        public BankAccount? BankAccount { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Date when payment was confirmed/cleared
        /// </summary>
        public DateTimeOffset? PaidAt { get; set; }

        /// <summary>
        /// Additional notes about the payment
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Collection of payment applications (allocations to invoices and advances)
        /// </summary>
        public ICollection<PaymentApplicationBase> Applications { get; set; } = new List<PaymentApplicationBase>();
    }
}
