using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class TenantAccountingSettingsService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<TenantAccountingSettings>> logger)
    : BaseService<TenantAccountingSettings>(context, logger)
{
    public override DbSet<TenantAccountingSettings> GetDbSet()
    {
        return Context.TenantAccountingSettings;
    }

    /// <summary>
    /// Gets the accounting settings for the current tenant with all navigation properties loaded.
    /// </summary>
    public async Task<TenantAccountingSettings?> GetSettingsForCurrentTenantAsync()
    {
        return await GetDbSet()
            .Include(s => s.DefaultSalesAccount)
            .Include(s => s.SalesReturnAccount)
            .Include(s => s.SalesDiscountAccount)
            .Include(s => s.DefaultPurchasesAccount)
            .Include(s => s.PurchaseReturnAccount)
            .Include(s => s.PurchaseDiscountAccount)
            .Include(s => s.DefaultTaxPayableAccount)
            .Include(s => s.DefaultSGSTPayableAccount)
            .Include(s => s.DefaultCGSTPayableAccount)
            .Include(s => s.DefaultIGSTPayableAccount)
            .Include(s => s.DefaultTaxInputAccount)
            .Include(s => s.DefaultSGSTInputAccount)
            .Include(s => s.DefaultCGSTInputAccount)
            .Include(s => s.DefaultIGSTInputAccount)
            .Include(s => s.DefaultCashAccount)
            .Include(s => s.DefaultBankAccount)
            .Include(s => s.DefaultCardAccount)
            .Include(s => s.DefaultUpiAccount)
            .Include(s => s.DefaultReceivableAccount)
            .Include(s => s.DefaultPayableAccount)
            .Include(s => s.DefaultInventoryAccount)
            .Include(s => s.WorkInProgressAccount)
            .Include(s => s.CostOfGoodsSoldAccount)
            .Include(s => s.RoundingAccount)
            .Include(s => s.ExchangeGainLossAccount)
            .Include(s => s.FreightChargesAccount)
            .Include(s => s.PackingChargesAccount)
            .Where(s => s.TenantId == TenantId && s.IsActive && s.IsNotDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets the accounting settings for a specific tenant with all navigation properties loaded.
    /// </summary>
    public async Task<TenantAccountingSettings?> GetSettingsForTenantAsync(int? tenantId)
    {
        return await GetDbSet()
            .Include(s => s.DefaultSalesAccount)
            .Include(s => s.SalesReturnAccount)
            .Include(s => s.SalesDiscountAccount)
            .Include(s => s.DefaultPurchasesAccount)
            .Include(s => s.PurchaseReturnAccount)
            .Include(s => s.PurchaseDiscountAccount)
            .Include(s => s.DefaultTaxPayableAccount)
            .Include(s => s.DefaultSGSTPayableAccount)
            .Include(s => s.DefaultCGSTPayableAccount)
            .Include(s => s.DefaultIGSTPayableAccount)
            .Include(s => s.DefaultTaxInputAccount)
            .Include(s => s.DefaultSGSTInputAccount)
            .Include(s => s.DefaultCGSTInputAccount)
            .Include(s => s.DefaultIGSTInputAccount)
            .Include(s => s.DefaultCashAccount)
            .Include(s => s.DefaultBankAccount)
            .Include(s => s.DefaultCardAccount)
            .Include(s => s.DefaultUpiAccount)
            .Include(s => s.DefaultReceivableAccount)
            .Include(s => s.DefaultPayableAccount)
            .Include(s => s.DefaultInventoryAccount)
            .Include(s => s.WorkInProgressAccount)
            .Include(s => s.CostOfGoodsSoldAccount)
            .Include(s => s.RoundingAccount)
            .Include(s => s.ExchangeGainLossAccount)
            .Include(s => s.FreightChargesAccount)
            .Include(s => s.PackingChargesAccount)
            .Where(s => s.TenantId == tenantId && s.IsActive && s.IsNotDeleted)
            .FirstOrDefaultAsync();
    }
}
