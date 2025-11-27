using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class BankAccount : LedgerAccount
    {
        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RoutingNumber { get; set; }

        [StringLength(20)]
        public string? SwiftCode { get; set; }

        [StringLength(50)]
        public string? IFSCCode { get; set; }

        [StringLength(100)]
        public string? BranchName { get; set; }

        [StringLength(50)]
        public string? AccountHolderName { get; set; }

        [StringLength(50)]
        public string? Currency { get; set; } = "INR";

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

        public DateTimeOffset? LastReconciliationDate { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
