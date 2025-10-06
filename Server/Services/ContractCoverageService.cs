using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Server.Services;

/// <summary>
/// Service for managing contract coverage rules and applying them to material usage
/// Handles AMC contract component coverage logic
/// </summary>
public class ContractCoverageService(
    VanigamAccountingDbContext context,
    ILogger<ContractCoverageService> logger)
{
    /// <summary>
    /// Get active contract for customer on specific date
    /// </summary>
    public async Task<Contract?> GetActiveContractAsync(Guid customerId, DateTimeOffset date)
    {
        return await context.Contracts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c =>
                c.CustomerId == customerId &&
                c.StartDate.HasValue && c.StartDate.Value <= date &&
                c.EndDate.HasValue && c.EndDate.Value >= date &&
                c.IsActive &&
                c.IsNotDeleted);
    }

    /// <summary>
    /// Get all coverage rules for a contract
    /// </summary>
    public async Task<List<ContractCoverageRule>> GetCoverageRulesAsync(Guid contractId)
    {
        return await context.ContractCoverageRules
            .Include(r => r.InventoryItem)
            .Where(r => r.ContractId == contractId && r.IsNotDeleted)
            .OrderBy(r => r.Priority)
            .ToListAsync();
    }

    /// <summary>
    /// Get applicable coverage rule for specific item
    /// Priority: 1) Exact item match, 2) Category match, 3) Default rule
    /// </summary>
    public async Task<ContractCoverageRule?> GetApplicableCoverageRuleAsync(
        Guid contractId,
        Guid inventoryItemId)
    {
        var rules = await GetCoverageRulesAsync(contractId);
        if (!rules.Any())
        {
            logger.LogInformation("No coverage rules found for contract {ContractId}", contractId);
            return null;
        }

        // Priority 1: Exact item match
        var itemRule = rules.FirstOrDefault(r => r.InventoryItemId == inventoryItemId);
        if (itemRule != null)
        {
            logger.LogInformation("Found exact item rule for item {ItemId} in contract {ContractId}",
                inventoryItemId, contractId);
            return itemRule;
        }

        // Priority 2: Category match (if item has category)
        var item = await context.InventoryItems.FindAsync(inventoryItemId);
        if (item?.CategoryId != null)
        {
            var categoryRule = rules.FirstOrDefault(r => r.InventoryItemCategoryId == item.CategoryId);
            if (categoryRule != null)
            {
                logger.LogInformation("Found category rule for item {ItemId} (category {CategoryId}) in contract {ContractId}",
                    inventoryItemId, item.CategoryId, contractId);
                return categoryRule;
            }
        }

        // Priority 3: Default rule (both InventoryItemId and CategoryId are null)
        var defaultRule = rules.FirstOrDefault(r =>
            r.InventoryItemId == null &&
            r.InventoryItemCategoryId == null);

        if (defaultRule != null)
        {
            logger.LogInformation("Using default rule for item {ItemId} in contract {ContractId}",
                inventoryItemId, contractId);
        }
        else
        {
            logger.LogWarning("No applicable rule found for item {ItemId} in contract {ContractId}",
                inventoryItemId, contractId);
        }

        return defaultRule;
    }

    /// <summary>
    /// Get current year usage for contract + item using database query
    /// </summary>
    public async Task<ContractItemUsageDTO> GetItemUsageAsync(
        Guid contractId,
        Guid inventoryItemId,
        int? year = null)
    {
        year ??= DateTime.UtcNow.Year;

        try
        {
            // Use raw SQL to query usage data
            // This works for both PostgreSQL and SQL Server with minor syntax differences
            var isPostgreSQL = context.Database.IsNpgsql();

            FormattableString query;

            if (isPostgreSQL)
            {
                query = $@"SELECT
                    COALESCE(SUM(mu.""Quantity""), 0) AS ""QuantityUsed"",
                    COALESCE(SUM(CAST(mu.""Quantity"" AS DECIMAL) * mu.""UnitPrice""), 0) AS ""ValueUsed"",
                    COALESCE(SUM(mu.""ChargedAmount""), 0) AS ""ChargedAmount"",
                    COALESCE(SUM(mu.""WaivedAmount""), 0) AS ""WaivedAmount"",
                    COALESCE(COUNT(*), 0) AS ""UsageCount""
                FROM ""Contracts"" c
                INNER JOIN ""Jobs"" j ON j.""PartyId"" = c.""CustomerId""
                    AND EXTRACT(YEAR FROM j.""CreatedAtUtc"") = {year}
                    AND j.""CreatedAtUtc"" BETWEEN c.""StartDate"" AND c.""EndDate""
                    AND j.""IsNotDeleted"" = true
                INNER JOIN ""MaterialUsages"" mu ON mu.""VoucherId"" = j.""Oid""
                    AND mu.""ItemId"" = {inventoryItemId}
                    AND mu.""IsNotDeleted"" = true
                WHERE c.""Oid"" = {contractId}
                    AND c.""IsNotDeleted"" = true
                    AND c.""IsActive"" = true";
            }
            else
            {
                query = $@"SELECT
                    COALESCE(SUM(mu.[Quantity]), 0) AS [QuantityUsed],
                    COALESCE(SUM(CAST(mu.[Quantity] AS DECIMAL) * mu.[UnitPrice]), 0) AS [ValueUsed],
                    COALESCE(SUM(mu.[ChargedAmount]), 0) AS [ChargedAmount],
                    COALESCE(SUM(mu.[WaivedAmount]), 0) AS [WaivedAmount],
                    COALESCE(COUNT(*), 0) AS [UsageCount]
                FROM [Contracts] c
                INNER JOIN [Jobs] j ON j.[PartyId] = c.[CustomerId]
                    AND YEAR(j.[CreatedAtUtc]) = {year}
                    AND j.[CreatedAtUtc] BETWEEN c.[StartDate] AND c.[EndDate]
                    AND j.[IsNotDeleted] = 1
                INNER JOIN [MaterialUsages] mu ON mu.[VoucherId] = j.[Oid]
                    AND mu.[ItemId] = {inventoryItemId}
                    AND mu.[IsNotDeleted] = 1
                WHERE c.[Oid] = {contractId}
                    AND c.[IsNotDeleted] = 1
                    AND c.[IsActive] = 1";
            }

            var usage = await context.Database
                .SqlQuery<ContractItemUsageDTO>(query)
                .FirstOrDefaultAsync();

            return usage ?? new ContractItemUsageDTO();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting item usage for contract {ContractId}, item {ItemId}, year {Year}",
                contractId, inventoryItemId, year);
            return new ContractItemUsageDTO();
        }
    }

    /// <summary>
    /// Get total contract usage (all items) for a year
    /// </summary>
    public async Task<ContractTotalUsageDTO> GetContractTotalUsageAsync(
        Guid contractId,
        int? year = null)
    {
        year ??= DateTime.UtcNow.Year;

        try
        {
            var isPostgreSQL = context.Database.IsNpgsql();

            FormattableString query;

            if (isPostgreSQL)
            {
                query = $@"SELECT
                    COALESCE(SUM(CAST(mu.""Quantity"" AS DECIMAL) * mu.""UnitPrice""), 0) AS ""TotalValueUsed"",
                    COALESCE(SUM(mu.""ChargedAmount""), 0) AS ""TotalChargedAmount"",
                    COALESCE(SUM(mu.""WaivedAmount""), 0) AS ""TotalWaivedAmount"",
                    COALESCE(COUNT(DISTINCT mu.""ItemId""), 0) AS ""ItemCount"",
                    COALESCE(COUNT(*), 0) AS ""UsageCount""
                FROM ""Contracts"" c
                INNER JOIN ""Jobs"" j ON j.""PartyId"" = c.""CustomerId""
                    AND EXTRACT(YEAR FROM j.""CreatedAtUtc"") = {year}
                    AND j.""CreatedAtUtc"" BETWEEN c.""StartDate"" AND c.""EndDate""
                    AND j.""IsNotDeleted"" = true
                INNER JOIN ""MaterialUsages"" mu ON mu.""VoucherId"" = j.""Oid""
                    AND mu.""IsNotDeleted"" = true
                WHERE c.""Oid"" = {contractId}
                    AND c.""IsNotDeleted"" = true
                    AND c.""IsActive"" = true";
            }
            else
            {
                query = $@"SELECT
                    COALESCE(SUM(CAST(mu.[Quantity] AS DECIMAL) * mu.[UnitPrice]), 0) AS [TotalValueUsed],
                    COALESCE(SUM(mu.[ChargedAmount]), 0) AS [TotalChargedAmount],
                    COALESCE(SUM(mu.[WaivedAmount]), 0) AS [TotalWaivedAmount],
                    COALESCE(COUNT(DISTINCT mu.[ItemId]), 0) AS [ItemCount],
                    COALESCE(COUNT(*), 0) AS [UsageCount]
                FROM [Contracts] c
                INNER JOIN [Jobs] j ON j.[PartyId] = c.[CustomerId]
                    AND YEAR(j.[CreatedAtUtc]) = {year}
                    AND j.[CreatedAtUtc] BETWEEN c.[StartDate] AND c.[EndDate]
                    AND j.[IsNotDeleted] = 1
                INNER JOIN [MaterialUsages] mu ON mu.[VoucherId] = j.[Oid]
                    AND mu.[IsNotDeleted] = 1
                WHERE c.[Oid] = {contractId}
                    AND c.[IsNotDeleted] = 1
                    AND c.[IsActive] = 1";
            }

            var usage = await context.Database
                .SqlQuery<ContractTotalUsageDTO>(query)
                .FirstOrDefaultAsync();

            return usage ?? new ContractTotalUsageDTO();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total usage for contract {ContractId}, year {Year}",
                contractId, year);
            return new ContractTotalUsageDTO();
        }
    }

    /// <summary>
    /// Apply coverage rule to material and calculate charged/waived amounts
    /// </summary>
    public async Task<MaterialCoverageResult> ApplyCoverageAsync(
        Guid contractId,
        Guid inventoryItemId,
        decimal quantity,
        decimal unitPrice,
        int? year = null)
    {
        var result = new MaterialCoverageResult
        {
            OriginalAmount = quantity * unitPrice,
            ChargedAmount = quantity * unitPrice,
            WaivedAmount = 0,
            ChargePercentage = 100,
            CoverageApplied = false,
            LimitExceeded = false
        };

        // Get coverage rule
        var rule = await GetApplicableCoverageRuleAsync(contractId, inventoryItemId);
        if (rule == null)
        {
            result.Notes = "No coverage rule found - charging full price";
            logger.LogInformation("No coverage rule found for contract {ContractId}, item {ItemId}",
                contractId, inventoryItemId);
            return result;
        }

        result.CoverageRuleId = rule.Oid;
        result.CoverageApplied = true;

        // Check usage limits
        var currentUsage = await GetItemUsageAsync(contractId, inventoryItemId, year);

        // Check quantity limit
        bool quantityExceeded = rule.MaxQuantityPerYear.HasValue &&
            (currentUsage.QuantityUsed + quantity) > rule.MaxQuantityPerYear.Value;

        // Check value limit
        bool valueExceeded = rule.MaxValuePerYear.HasValue &&
            (currentUsage.ValueUsed + result.OriginalAmount) > rule.MaxValuePerYear.Value;

        if (quantityExceeded || valueExceeded)
        {
            // Limit exceeded - charge full price
            result.ChargedAmount = result.OriginalAmount;
            result.WaivedAmount = 0;
            result.ChargePercentage = 100;
            result.LimitExceeded = true;
            result.Notes = quantityExceeded
                ? $"Quantity limit exceeded ({rule.MaxQuantityPerYear}/year) - Full charge applies"
                : $"Value limit exceeded ({rule.MaxValuePerYear:C}/year) - Full charge applies";

            logger.LogWarning("Coverage limit exceeded for contract {ContractId}, item {ItemId}: {Reason}",
                contractId, inventoryItemId, result.Notes);

            return result;
        }

        // Apply coverage percentage
        var chargePercentage = rule.CoverageType switch
        {
            CoverageRuleType.Free => 0m,
            CoverageRuleType.Chargeable => 100m,
            CoverageRuleType.PartiallyChargeable => rule.ChargePercentage,
            _ => 100m
        };

        result.ChargePercentage = chargePercentage;
        result.ChargedAmount = result.OriginalAmount * (chargePercentage / 100);
        result.WaivedAmount = result.OriginalAmount - result.ChargedAmount;

        result.Notes = chargePercentage == 0
            ? $"Fully covered under AMC Contract"
            : chargePercentage == 100
                ? "Chargeable item - Not covered by contract"
                : $"{100 - chargePercentage:F0}% discount applied ({chargePercentage:F0}% charged)";

        logger.LogInformation("Coverage applied for contract {ContractId}, item {ItemId}: {ChargePercentage}% charged, {Notes}",
            contractId, inventoryItemId, chargePercentage, result.Notes);

        return result;
    }
}
