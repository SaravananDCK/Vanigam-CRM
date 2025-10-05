using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Represents material/inventory item usage in a job
    /// Supports contract coverage tracking for AMC scenarios
    /// </summary>
    public class MaterialUsage : VoucherLine
    {
        /// <summary>
        /// Contract coverage rule applied to this material usage (null if no contract)
        /// </summary>
        public Guid? ContractCoverageRuleId { get; set; }

        [ForeignKey(nameof(ContractCoverageRuleId))]
        public ContractCoverageRule? ContractCoverageRuleApplied { get; set; }

        /// <summary>
        /// Amount charged to customer (after contract discount/waiver)
        /// This is what appears on the invoice
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ChargedAmount { get; set; }

        /// <summary>
        /// Amount waived/covered by contract
        /// WaivedAmount + ChargedAmount = TotalValue (Quantity * UnitPrice)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal WaivedAmount { get; set; }

        /// <summary>
        /// Notes about coverage applied (e.g., "Covered under AMC Contract #12345")
        /// </summary>
        [StringLength(200)]
        public string? CoverageNotes { get; set; }

        /// <summary>
        /// Total value before any contract coverage (calculated property)
        /// </summary>
        [NotMapped]
        public decimal TotalValue => (decimal)Quantity * UnitPrice;
    }
}
