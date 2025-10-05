using System.ComponentModel.DataAnnotations;

namespace Vanigam.CRM.Objects.DTOs;

/// <summary>
/// DTO for querying contract item usage from database
/// </summary>
public class ContractItemUsageDTO
{
    public decimal QuantityUsed { get; set; }
    public decimal ValueUsed { get; set; }
    public decimal ChargedAmount { get; set; }
    public decimal WaivedAmount { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// DTO for querying total contract usage (all items) from database
/// </summary>
public class ContractTotalUsageDTO
{
    public decimal TotalValueUsed { get; set; }
    public decimal TotalChargedAmount { get; set; }
    public decimal TotalWaivedAmount { get; set; }
    public int ItemCount { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// Result of applying coverage rules to a material
/// </summary>
public class MaterialCoverageResult
{
    /// <summary>
    /// ID of the coverage rule that was applied
    /// </summary>
    public Guid? CoverageRuleId { get; set; }

    /// <summary>
    /// Original amount before coverage (Quantity * UnitPrice)
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Amount to charge customer (after coverage discount)
    /// </summary>
    public decimal ChargedAmount { get; set; }

    /// <summary>
    /// Amount waived/covered by contract
    /// </summary>
    public decimal WaivedAmount { get; set; }

    /// <summary>
    /// Whether any coverage was applied
    /// </summary>
    public bool CoverageApplied { get; set; }

    /// <summary>
    /// Whether usage limits were exceeded
    /// </summary>
    public bool LimitExceeded { get; set; }

    /// <summary>
    /// Percentage customer pays (0-100)
    /// </summary>
    public decimal ChargePercentage { get; set; }

    /// <summary>
    /// Notes about coverage (e.g., "Covered under AMC", "Limit exceeded")
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request to apply coverage to a material
/// </summary>
public class ApplyCoverageRequest
{
    [Required]
    public Guid ContractId { get; set; }

    [Required]
    public Guid InventoryItemId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public int? Year { get; set; } // Defaults to current year
}
