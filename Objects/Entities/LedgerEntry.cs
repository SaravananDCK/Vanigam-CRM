using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NodaTime;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class LedgerEntry : BaseClass
    {
        public DateTimeOffset EntryDate { get; set; } = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

        [Required]
        [StringLength(50)]
        public string EntryNumber { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EntryType EntryType { get; set; } // Debit or Credit

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        public Guid? VoucherId { get; set; }
        [ForeignKey(nameof(VoucherId))]
        public Voucher? Voucher { get; set; }

        // Single account reference - Entry is either Debit or Credit to this account
        [Required]
        public Guid AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public LedgerAccount Account { get; set; }

        public bool IsReconciled { get; set; } = false;

        public DateTimeOffset? ReconciledDate { get; set; }

        [StringLength(200)]
        public string? ReconciledBy { get; set; }

        // PostgreSQL generated columns for reporting
        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; private set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; private set; }
    }
}
