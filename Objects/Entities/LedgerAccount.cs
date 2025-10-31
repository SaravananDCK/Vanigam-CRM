using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    
    // Base class for all ledger accounts using Table-Per-Hierarchy (TPH) inheritance
    public class LedgerAccount : BaseClass
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType AccountType { get; set; }

        [StringLength(20)]
        public string? GSTIN { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 0;

        [StringLength(2000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? AccountGroupId { get; set; }
        [ForeignKey(nameof(AccountGroupId))]
        public AccountGroup? AccountGroup { get; set; }

        // Navigation property - Single collection for all transactions (both debit and credit)
        public ICollection<LedgerEntry> Transactions { get; set; } = new List<LedgerEntry>();
        public ICollection<FileDocument> FileDocuments { get; set; } = new List<FileDocument>();
    }
}
