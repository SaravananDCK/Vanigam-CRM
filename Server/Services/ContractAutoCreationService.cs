using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

/// <summary>
/// Service responsible for automatic creation of Warranty, Guarantee, and AMC contracts
/// when invoices are posted. Implements idempotent processing with duplication safeguards.
/// </summary>
public class ContractAutoCreationService(
    VanigamAccountingDbContext context,
    ILogger<ContractAutoCreationService> logger,
    IConfiguration configuration)
{
    private bool IsEnabled => configuration.GetValue<bool>("ContractAutoCreation:Enabled", true);
    private bool PreventDuplicates => configuration.GetValue<bool>("ContractAutoCreation:PreventDuplicates", true);
    private bool LogActivity => configuration.GetValue<bool>("ContractAutoCreation:LogActivity", true);
    private bool GroupByWarrantyPeriod => configuration.GetValue<bool>("ContractAutoCreation:GroupItemsByWarrantyPeriod", true);

    #region Main Entry Point

    /// <summary>
    /// Processes an invoice for automatic contract creation.
    /// Idempotent - safe to call multiple times for the same invoice.
    /// </summary>
    public async Task ProcessInvoiceForContracts(Invoice invoice)
    {
        if (!IsEnabled)
        {
            logger.LogInformation("Contract auto-creation is disabled - skipping Invoice {InvoiceNumber}", invoice.Number);
            return;
        }

        try
        {
            logger.LogInformation("Processing Invoice {InvoiceNumber} for contract auto-creation", invoice.Number);

            // SAFEGUARD: Check if contracts already exist for this invoice
            if (PreventDuplicates && await HasExistingContracts(invoice.Oid))
            {
                logger.LogInformation("Contracts already exist for Invoice {InvoiceNumber} - skipping", invoice.Number);
                return;
            }

            // Load invoice with items and item details
            var invoiceWithItems = await context.Invoices
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Item)
                .FirstOrDefaultAsync(i => i.Oid == invoice.Oid);

            if (invoiceWithItems == null || !invoiceWithItems.Items.Any())
            {
                logger.LogInformation("Invoice {InvoiceNumber} has no items - skipping contract creation", invoice.Number);
                return;
            }

            // Create Warranty Contracts
            await CreateWarrantyContracts(invoiceWithItems);

            // Create Guarantee Contracts
            await CreateGuaranteeContracts(invoiceWithItems);

            // Create AMC Quotes (optional - for future revenue)
            // await CreateAmcQuotes(invoiceWithItems);

            logger.LogInformation("Successfully processed Invoice {InvoiceNumber} for contracts", invoice.Number);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Invoice {InvoiceNumber} for contracts", invoice.Number);
            throw;
        }
    }

    #endregion

    #region Warranty Contract Creation

    /// <summary>
    /// Creates warranty contracts for eligible invoice items.
    /// Groups items by warranty period if configured.
    /// </summary>
    private async Task CreateWarrantyContracts(Invoice invoice)
    {
        // Get items with warranty
        var itemsWithWarranty = invoice.Items
            .Where(ii => ii.Item != null && ii.Item.WarrantyPeriodMonths.HasValue && ii.Item.WarrantyPeriodMonths > 0)
            .ToList();

        if (!itemsWithWarranty.Any())
        {
            logger.LogDebug("No items with warranty found in Invoice {InvoiceNumber}", invoice.Number);
            return;
        }

        if (GroupByWarrantyPeriod)
        {
            // Group by warranty period
            var groupedItems = itemsWithWarranty
                .GroupBy(ii => ii.Item!.WarrantyPeriodMonths!.Value);

            foreach (var group in groupedItems)
            {
                await CreateWarrantyContractForGroup(invoice, group.Key, group.ToList());
            }
        }
        else
        {
            // Single contract for all items
            var warrantyPeriod = itemsWithWarranty.Max(ii => ii.Item!.WarrantyPeriodMonths!.Value);
            await CreateWarrantyContractForGroup(invoice, warrantyPeriod, itemsWithWarranty);
        }
    }

    /// <summary>
    /// Creates a warranty contract for a group of items with the same warranty period
    /// </summary>
    private async Task CreateWarrantyContractForGroup(Invoice invoice, int warrantyMonths, List<InvoiceItem> items)
    {
        var contract = new WarrantyContract
        {
            Oid = Guid.NewGuid(),
            TenantId = invoice.TenantId,
            Title = $"Warranty - {invoice.Number}",
            CustomerId = invoice.PartyId,
            InvoiceId = invoice.Oid,
            StartDate = invoice.VoucherDate,
            Duration = CalculateContractDuration(warrantyMonths),
            CoverageType = ContractCoverageType.FullCoverage,
            IncludesPartsReplacement = true,
            ContractValue = null, // Free warranty
            IsActive = true,
            AutoRenewEnabled = false,
            RequiresRegistration = false,
            IsTransferable = false,
            CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            IsNotDeleted = true
        };

        context.Contracts.Add(contract);
        await context.SaveChangesAsync(); // Save contract first to get ID

        // Create coverage rules for each item
        foreach (var item in items)
        {
            var coverageRule = new ContractCoverageRule
            {
                Oid = Guid.NewGuid(),
                TenantId = invoice.TenantId,
                ContractId = contract.Oid,
                InvoiceItemId = item.Oid,
                InventoryItemId = item.ItemId,
                CoverageType = CoverageRuleType.Free,
                ChargePercentage = 0,
                Priority = 1, // Item-specific rules have highest priority
                CoveredQuantity = item.Quantity,
                ItemWarrantyEndDate = invoice.VoucherDate.AddMonths(warrantyMonths),
                Notes = $"Auto-generated warranty coverage for {item.Item?.Name}",
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            context.ContractCoverageRules.Add(coverageRule);
        }

        await context.SaveChangesAsync();

        // Log activity
        if (LogActivity)
        {
            await CreateActivity(invoice, contract, $"Warranty contract created with {items.Count} item(s) for {warrantyMonths} months");
        }

        logger.LogInformation("Created Warranty contract {ContractId} for Invoice {InvoiceNumber} with {ItemCount} items ({Months} months)",
            contract.Oid, invoice.Number, items.Count, warrantyMonths);
    }

    #endregion

    #region Guarantee Contract Creation

    /// <summary>
    /// Creates guarantee contracts for eligible invoice items.
    /// Guarantee typically starts after warranty ends.
    /// </summary>
    private async Task CreateGuaranteeContracts(Invoice invoice)
    {
        // Get items with guarantee period
        var itemsWithGuarantee = invoice.Items
            .Where(ii => ii.Item != null && ii.Item.GuaranteePeriodMonths.HasValue && ii.Item.GuaranteePeriodMonths > 0)
            .ToList();

        if (!itemsWithGuarantee.Any())
        {
            logger.LogDebug("No items with guarantee found in Invoice {InvoiceNumber}", invoice.Number);
            return;
        }

        if (GroupByWarrantyPeriod)
        {
            // Group by guarantee period
            var groupedItems = itemsWithGuarantee
                .GroupBy(ii => new
                {
                    GuaranteeMonths = ii.Item!.GuaranteePeriodMonths!.Value,
                    WarrantyMonths = ii.Item.WarrantyPeriodMonths ?? 0
                });

            foreach (var group in groupedItems)
            {
                await CreateGuaranteeContractForGroup(invoice, group.Key.GuaranteeMonths, group.Key.WarrantyMonths, group.ToList());
            }
        }
        else
        {
            // Single contract for all items
            var maxWarranty = itemsWithGuarantee.Max(ii => ii.Item!.WarrantyPeriodMonths ?? 0);
            var maxGuarantee = itemsWithGuarantee.Max(ii => ii.Item!.GuaranteePeriodMonths!.Value);
            await CreateGuaranteeContractForGroup(invoice, maxGuarantee, maxWarranty, itemsWithGuarantee);
        }
    }

    /// <summary>
    /// Creates a guarantee contract for a group of items
    /// </summary>
    private async Task CreateGuaranteeContractForGroup(Invoice invoice, int guaranteeMonths, int warrantyMonths, List<InvoiceItem> items)
    {
        // Guarantee starts after warranty ends
        var startDate = invoice.VoucherDate.AddMonths(warrantyMonths);

        var contract = new GuaranteeContract
        {
            Oid = Guid.NewGuid(),
            TenantId = invoice.TenantId,
            Title = $"Guarantee - {invoice.Number}",
            CustomerId = invoice.PartyId,
            InvoiceId = invoice.Oid,
            StartDate = startDate,
            Duration = CalculateContractDuration(guaranteeMonths),
            CoverageType = ContractCoverageType.FullCoverage,
            IncludesPartsReplacement = true,
            ContractValue = null, // Can be configured if guarantee is paid
            IsActive = true,
            AutoRenewEnabled = false,
            IsMoneyBackGuarantee = false,
            CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            IsNotDeleted = true
        };

        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Create coverage rules for each item
        foreach (var item in items)
        {
            var coverageRule = new ContractCoverageRule
            {
                Oid = Guid.NewGuid(),
                TenantId = invoice.TenantId,
                ContractId = contract.Oid,
                InvoiceItemId = item.Oid,
                InventoryItemId = item.ItemId,
                CoverageType = CoverageRuleType.Free,
                ChargePercentage = 0,
                Priority = 1,
                CoveredQuantity = item.Quantity,
                ItemWarrantyEndDate = startDate.AddMonths(guaranteeMonths),
                Notes = $"Auto-generated guarantee coverage for {item.Item?.Name}",
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            context.ContractCoverageRules.Add(coverageRule);
        }

        await context.SaveChangesAsync();

        // Log activity
        if (LogActivity)
        {
            await CreateActivity(invoice, contract, $"Guarantee contract created with {items.Count} item(s) for {guaranteeMonths} months");
        }

        logger.LogInformation("Created Guarantee contract {ContractId} for Invoice {InvoiceNumber} with {ItemCount} items ({Months} months)",
            contract.Oid, invoice.Number, items.Count, guaranteeMonths);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if contracts already exist for this invoice (duplication prevention)
    /// </summary>
    private async Task<bool> HasExistingContracts(Guid invoiceId)
    {
        return await context.Contracts
            .AnyAsync(c => c.InvoiceId == invoiceId && c.IsNotDeleted);
    }

    /// <summary>
    /// Converts warranty months to ContractDuration enum
    /// </summary>
    private ContractDuration CalculateContractDuration(int months)
    {
        return months switch
        {
            <= 3 => ContractDuration.Quarterly,
            <= 6 => ContractDuration.SemiAnnual,
            <= 12 => ContractDuration.Annual,
            <= 24 => ContractDuration.Biennial,
            <= 36 => ContractDuration.Triennial,
            _ => ContractDuration.Triennial // Max available is 36 months
        };
    }

    /// <summary>
    /// Creates an activity log entry for contract creation
    /// </summary>
    private async Task CreateActivity(Invoice invoice, Contract contract, string description)
    {
        var activity = new Activity
        {
            Oid = Guid.NewGuid(),
            TenantId = invoice.TenantId,
            Type = ActivityType.Note, // Using Note type for contract creation tracking
            Subject = contract.Title,
            Description = description,
            ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            Status = ActivityStatus.Completed,
            CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
            IsNotDeleted = true
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();
    }

    #endregion
}
