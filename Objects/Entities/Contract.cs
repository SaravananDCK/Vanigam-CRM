using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Contract : BaseClass
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Guid? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        [StringLength(5000)]
        public string? Terms { get; set; }

        /// <summary>
        /// Whether this contract is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Type of coverage provided by this contract
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractCoverageType CoverageType { get; set; } = ContractCoverageType.PartialCoverage;

        /// <summary>
        /// Whether parts replacement is included in this contract
        /// </summary>
        public bool IncludesPartsReplacement { get; set; } = true;

        /// <summary>
        /// Overall parts replacement limit per year (null = unlimited)
        /// This is in addition to per-item limits defined in coverage rules
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PartsReplacementLimit { get; set; }

        // Navigation properties
        public ICollection<RecurringJob> RecurringJobs { get; set; } = new List<RecurringJob>();
        public ICollection<ContractCoverageRule> CoverageRules { get; set; } = new List<ContractCoverageRule>();
    }
}
