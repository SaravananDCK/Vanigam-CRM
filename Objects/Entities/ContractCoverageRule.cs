using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Defines coverage rules for contract - which components are free, chargeable, or partially covered
    /// Used for AMC contracts to determine if material usage should be charged to customer
    /// </summary>
    public class ContractCoverageRule : BaseClass
    {
        [Required]
        public Guid ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        /// <summary>
        /// Rule applies to specific inventory item (null = applies to category or all items)
        /// </summary>
        public Guid? InventoryItemId { get; set; }

        [ForeignKey(nameof(InventoryItemId))]
        public InventoryItem? InventoryItem { get; set; }

        /// <summary>
        /// Rule applies to inventory item category (null = applies to all items)
        /// Note: If both InventoryItemId and CategoryId are null, this is the default rule
        /// </summary>
        public Guid? InventoryItemCategoryId { get; set; }

        [ForeignKey(nameof(InventoryItemCategoryId))]
        public ItemCategory? Category { get; set; }

        /// <summary>
        /// Type of coverage for this rule
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CoverageRuleType CoverageType { get; set; } = CoverageRuleType.Free;

        /// <summary>
        /// Percentage customer pays (0-100)
        /// 0 = Free (fully covered)
        /// 100 = Full price (chargeable)
        /// 50 = Customer pays 50%, contract covers 50%
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal ChargePercentage { get; set; } = 0;

        /// <summary>
        /// Maximum quantity per year allowed under this rule (null = unlimited)
        /// After limit exceeded, standard pricing applies
        /// </summary>
        public int? MaxQuantityPerYear { get; set; }

        /// <summary>
        /// Maximum total value per year allowed under this rule (null = unlimited)
        /// After limit exceeded, standard pricing applies
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxValuePerYear { get; set; }

        /// <summary>
        /// Rule priority for conflict resolution (lower number = higher priority)
        /// Item-specific rules typically have priority 1
        /// Category rules have priority 50
        /// Default rules have priority 100
        /// </summary>
        [Required]
        public int Priority { get; set; } = 100;

        [StringLength(500)]
        public string? Notes { get; set; }

        // ========== NEW FIELDS for Warranty Tracking ==========

        /// <summary>
        /// Link to specific invoice item (for warranty contracts)
        /// Allows tracking which invoice item this warranty rule covers
        /// </summary>
        public Guid? InvoiceItemId { get; set; }

        [ForeignKey(nameof(InvoiceItemId))]
        public InvoiceItem? InvoiceItem { get; set; }

        /// <summary>
        /// Serial number of the specific item covered (optional)
        /// Used for tracking individual product warranties
        /// </summary>
        [StringLength(100)]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Warranty/guarantee end date for this specific item (can override contract end date)
        /// Different products in same contract may have different warranty periods
        /// </summary>
        public DateTimeOffset? ItemWarrantyEndDate { get; set; }

        /// <summary>
        /// Quantity of this item covered (for warranty contracts from invoice)
        /// </summary>
        public double? CoveredQuantity { get; set; }
    }
}
