using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class NumberSeriesService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<NumberSeries>> logger)
    : BaseService<NumberSeries>(context, logger)
{
    public override DbSet<NumberSeries> GetDbSet()
    {
        return Context.NumberSeries;
    }

    /// <summary>
    /// Generates the next number in the series for the specified entity type
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Quote", "Invoice", "PurchaseOrder", "PurchaseInvoice")</param>
    /// <param name="tenantId">The tenant ID for multi-tenant filtering</param>
    /// <returns>The formatted next number (e.g., "QT-0001", "INV-0001")</returns>
    public async Task<string> GenerateNextNumber(string entityType, int? tenantId)
    {
        // Find the active number series for this entity type and tenant
        var numberSeries = await Context.NumberSeries
            .FirstOrDefaultAsync(ns => ns.EntityType == entityType
                && ns.IsActive
                && ns.TenantId == tenantId
                && ns.IsNotDeleted);

        if (numberSeries == null)
        {
            // Create a new number series with default settings
            numberSeries = new NumberSeries
            {
                Oid = Guid.NewGuid(),
                TenantId = tenantId ?? 0,
                EntityType = entityType,
                Prefix = GetDefaultPrefix(entityType),
                Suffix = null,
                StartNo = 1,
                CurrentNo = 1,
                PaddingLength = 4,
                IsActive = true,
                CreatedByUserId = ApplicationUser.SystemUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = ApplicationUser.SystemUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            await Context.NumberSeries.AddAsync(numberSeries);
            await Context.SaveChangesAsync();
        }

        // Get the current number and increment
        var currentNumber = numberSeries.CurrentNo;

        // Format the number with padding
        var paddedNumber = currentNumber.ToString().PadLeft(numberSeries.PaddingLength, '0');

        // Build the formatted number (Prefix + PaddedNumber + Suffix)
        var formattedNumber = $"{numberSeries.Prefix}{paddedNumber}{numberSeries.Suffix}";

        // Increment for next use
        numberSeries.CurrentNo++;
        numberSeries.UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

        // Save changes
        await Context.SaveChangesAsync();

        return formattedNumber;
    }

    /// <summary>
    /// Resets the number series to its starting number
    /// </summary>
    /// <param name="entityType">The entity type to reset</param>
    /// <param name="tenantId">The tenant ID for multi-tenant filtering</param>
    public async Task ResetNumberSeries(string entityType, int? tenantId)
    {
        var numberSeries = await Context.NumberSeries
            .FirstOrDefaultAsync(ns => ns.EntityType == entityType
                && ns.TenantId == tenantId
                && ns.IsNotDeleted);

        if (numberSeries != null)
        {
            numberSeries.CurrentNo = numberSeries.StartNo;
            numberSeries.UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
            await Context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets the preview of the next number without incrementing
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <param name="tenantId">The tenant ID for multi-tenant filtering</param>
    /// <returns>The formatted next number preview</returns>
    public async Task<string?> PreviewNextNumber(string entityType, int? tenantId)
    {
        var numberSeries = await Context.NumberSeries
            .FirstOrDefaultAsync(ns => ns.EntityType == entityType
                && ns.IsActive
                && ns.TenantId == tenantId
                && ns.IsNotDeleted);

        if (numberSeries == null)
            return null;

        var currentNumber = numberSeries.CurrentNo;
        var paddedNumber = currentNumber.ToString().PadLeft(numberSeries.PaddingLength, '0');
        return $"{numberSeries.Prefix}{paddedNumber}{numberSeries.Suffix}";
    }

    /// <summary>
    /// Gets the default prefix for an entity type
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <returns>The default prefix</returns>
    private static string GetDefaultPrefix(string entityType)
    {
        return entityType switch
        {
            "Quote" => "QT-",
            "Invoice" => "INV-",
            "PurchaseOrder" => "PO-",
            "PurchaseInvoice" => "PI-",
            "Customer" => "CUST-",
            "Vendor" => "VEND-",
            "BankAccount" => "BANK-",
            _ => $"{entityType.ToUpper().Substring(0, Math.Min(3, entityType.Length))}-"
        };
    }
}
