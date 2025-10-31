using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services;

/// <summary>
/// Service for generating GSTR-3B (Summary Return) report data
/// </summary>
public class GSTR3BReportService(
    VanigamAccountingDbContext context,
    ILogger<GSTR3BReportService> logger,
    ICurrentUserService currentUserService)
{
    /// <summary>
    /// Generates complete GSTR-3B report for the specified period
    /// </summary>
    public async Task<GSTR3BReport> GenerateGSTR3BReportAsync(int month, int year)
    {
        logger.LogInformation("Generating GSTR-3B report for period {Month}/{Year}", month, year);

        var tenantId = currentUserService.TenantId;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var report = new GSTR3BReport
        {
            GSTIN = await GetTenantGSTIN(tenantId),
            LegalName = await GetTenantLegalName(tenantId),
            ReturnPeriod = $"{month:D2}{year}"
        };

        // Table 3.1 - Outward Supplies (from Sales Invoices)
        report.OutwardSupplies = await CalculateOutwardSupplies(tenantId, startDate, endDate);

        // Table 3.2 - Inter-state Supplies
        report.InterStateSupplies = await CalculateInterStateSupplies(tenantId, startDate, endDate);

        // Table 4 - ITC Available (from Purchase Invoices)
        report.ITCAvailable = await CalculateITCAvailable(tenantId, startDate, endDate);

        // Table 5 - Exempt Supplies
        report.ExemptSupplies = await CalculateExemptSupplies(tenantId, startDate, endDate);

        // Table 6.1 - Tax Payable
        report.TaxPayable = CalculateTaxPayable(report.OutwardSupplies, report.ITCAvailable);

        logger.LogInformation("GSTR-3B report generated successfully. Total Tax Payable: CGST={CGST}, SGST={SGST}, IGST={IGST}",
            report.TaxPayable.TaxPayableVal.CGSTAmount,
            report.TaxPayable.TaxPayableVal.SGSTAmount,
            report.TaxPayable.TaxPayableVal.IGSTAmount);

        return report;
    }

    /// <summary>
    /// Calculate Table 3.1 - Total Outward Supplies
    /// </summary>
    private async Task<OutwardSupplies> CalculateOutwardSupplies(int? tenantId, DateTime startDate, DateTime endDate)
    {
        var invoices = await context.Invoices
            .Where(i =>
                i.TenantId == tenantId &&
                i.VoucherDate >= startDate &&
                i.VoucherDate <= endDate &&
                i.IsNotDeleted)
            .ToListAsync();

        return new OutwardSupplies
        {
            TaxableValue = invoices.Sum(i => i.SubTotal),
            CGSTAmount = invoices.Sum(i => i.CGSTAmount),
            SGSTAmount = invoices.Sum(i => i.SGSTAmount),
            IGSTAmount = invoices.Sum(i => i.IGSTAmount),
            CessAmount = invoices.Sum(i => i.CessAmount)
        };
    }

    /// <summary>
    /// Calculate Table 3.2 - Inter-state Supplies (State-wise)
    /// </summary>
    private async Task<List<InterStateSupply>> CalculateInterStateSupplies(int? tenantId, DateTime startDate, DateTime endDate)
    {
        var interStateSupplies = await context.Invoices
            .Include(i => i.Party)
            .Where(i =>
                i.TenantId == tenantId &&
                i.VoucherDate >= startDate &&
                i.VoucherDate <= endDate &&
                i.IGSTAmount > 0 && // Inter-state transactions have IGST
                i.IsNotDeleted)
            .GroupBy(i => i.Party!.State ?? "Unknown")
            .Select(g => new InterStateSupply
            {
                PlaceOfSupply = g.Key,
                TaxableValue = g.Sum(i => i.SubTotal),
                IGSTAmount = g.Sum(i => i.IGSTAmount)
            })
            .ToListAsync();

        return interStateSupplies;
    }

    /// <summary>
    /// Calculate Table 4 - ITC Available (Input Tax Credit)
    /// </summary>
    private async Task<ITCAvailable> CalculateITCAvailable(int? tenantId, DateTime startDate, DateTime endDate)
    {
        var purchaseInvoices = await context.PurchaseInvoices
            .Where(pi =>
                pi.TenantId == tenantId &&
                pi.VoucherDate >= startDate &&
                pi.VoucherDate <= endDate &&
                pi.IsNotDeleted)
            .ToListAsync();

        var totalITC = new TaxAmounts
        {
            CGSTAmount = purchaseInvoices.Sum(pi => pi.CGSTAmount),
            SGSTAmount = purchaseInvoices.Sum(pi => pi.SGSTAmount),
            IGSTAmount = purchaseInvoices.Sum(pi => pi.IGSTAmount),
            CessAmount = purchaseInvoices.Sum(pi => pi.CessAmount)
        };

        // For simplicity, assuming all ITC is from "All other ITC" category
        // In reality, you'd need to categorize by import of goods, services, ISD, etc.
        return new ITCAvailable
        {
            ImportOfGoods = new TaxAmounts(), // TODO: Identify import transactions
            ImportOfServices = new TaxAmounts(), // TODO: Identify service imports
            InwardFromISD = new TaxAmounts(), // TODO: Identify ISD transactions
            AllOtherITC = totalITC,
            TotalITC = totalITC,
            ITCReversed = new TaxAmounts(), // TODO: Implement ITC reversal logic
            NetITC = totalITC, // Net = Total - Reversed
            IneligibleITC = new TaxAmounts() // TODO: Implement ineligible ITC logic
        };
    }

    /// <summary>
    /// Calculate Table 5 - Exempt, Nil Rated and Non-GST Supplies
    /// </summary>
    private async Task<ExemptSupplies> CalculateExemptSupplies(int? tenantId, DateTime startDate, DateTime endDate)
    {
        // TODO: Implement logic to identify exempt/nil-rated supplies
        // For now, returning zeros
        return new ExemptSupplies
        {
            NilRated = 0,
            Exempted = 0,
            NonGST = 0
        };
    }

    /// <summary>
    /// Calculate Table 6.1 - Tax Payable and Paid
    /// </summary>
    private TaxPayable CalculateTaxPayable(OutwardSupplies outward, ITCAvailable itc)
    {
        // Tax payable = Output tax - Input tax credit
        var taxPayable = new TaxAmounts
        {
            CGSTAmount = Math.Max(0, outward.CGSTAmount - itc.NetITC.CGSTAmount),
            SGSTAmount = Math.Max(0, outward.SGSTAmount - itc.NetITC.SGSTAmount),
            IGSTAmount = Math.Max(0, outward.IGSTAmount - itc.NetITC.IGSTAmount),
            CessAmount = Math.Max(0, outward.CessAmount - itc.NetITC.CessAmount)
        };

        // ITC utilized for payment
        var itcUtilized = new TaxAmounts
        {
            CGSTAmount = Math.Min(itc.NetITC.CGSTAmount, outward.CGSTAmount),
            SGSTAmount = Math.Min(itc.NetITC.SGSTAmount, outward.SGSTAmount),
            IGSTAmount = Math.Min(itc.NetITC.IGSTAmount, outward.IGSTAmount),
            CessAmount = Math.Min(itc.NetITC.CessAmount, outward.CessAmount)
        };

        // Cash payment = Tax payable (after ITC set-off)
        var cashPaid = new TaxAmounts
        {
            CGSTAmount = taxPayable.CGSTAmount,
            SGSTAmount = taxPayable.SGSTAmount,
            IGSTAmount = taxPayable.IGSTAmount,
            CessAmount = taxPayable.CessAmount
        };

        return new TaxPayable
        {
            TaxPayableVal = new TaxAmounts
            {
                CGSTAmount = outward.CGSTAmount,
                SGSTAmount = outward.SGSTAmount,
                IGSTAmount = outward.IGSTAmount,
                CessAmount = outward.CessAmount
            },
            TaxPaidThroughITC = itcUtilized,
            TaxPaidInCash = cashPaid,
            Interest = 0, // TODO: Calculate interest on late payment
            LateFee = 0 // TODO: Calculate late fee if applicable
        };
    }

    #region Helper Methods

    private async Task<string> GetTenantGSTIN(int? tenantId)
    {
        var tenant = await context.TenantAccountingSettings
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        return tenant?.GSTIN ?? string.Empty;
    }

    private async Task<string> GetTenantLegalName(int? tenantId)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);
        return tenant?.Name ?? string.Empty;
    }

    #endregion
}
