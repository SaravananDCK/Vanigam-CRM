using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services;

/// <summary>
/// Service for generating GSTR-1 (Outward Supplies) report data
/// </summary>
public class GSTR1ReportService(
    VanigamAccountingDbContext context,
    ILogger<GSTR1ReportService> logger,
    ICurrentUserService currentUserService)
{
    private const decimal B2C_LARGE_INVOICE_THRESHOLD = 250000; // ₹2.5 lakh

    /// <summary>
    /// Generates complete GSTR-1 report for the specified period
    /// </summary>
    public async Task<GSTR1Report> GenerateGSTR1ReportAsync(int month, int year)
    {
        logger.LogInformation("Generating GSTR-1 report for period {Month}/{Year}", month, year);

        var tenantId = currentUserService.TenantId;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var report = new GSTR1Report
        {
            GSTIN = await GetTenantGSTIN(tenantId),
            LegalName = await GetTenantLegalName(tenantId),
            TradeName = await GetTenantTradeName(tenantId),
            ReturnPeriod = $"{month:D2}{year}"
        };

        // Get all invoices for the period
        var invoices = await GetInvoicesForPeriod(tenantId, startDate, endDate);

        // Generate B2B invoices (Business to Business)
        report.B2BInvoices = await GenerateB2BInvoices(invoices);

        // Generate B2CL invoices (Large B2C transactions > 2.5 lakh)
        report.B2CLInvoices = await GenerateB2CLInvoices(invoices);

        // Generate B2CS invoices (Small B2C transactions - state-wise summary)
        report.B2CSInvoices = await GenerateB2CSInvoices(invoices);

        // Generate Export invoices
        report.ExportInvoices = await GenerateExportInvoices(invoices);

        // Generate HSN Summary
        report.HSNSummary = await GenerateHSNSummary(tenantId, startDate, endDate);

        logger.LogInformation("GSTR-1 report generated successfully. B2B: {B2BCount}, HSN: {HSNCount}",
            report.B2BInvoices.Count, report.HSNSummary.Count);

        return report;
    }

    /// <summary>
    /// Generate B2B (Business to Business) invoice details
    /// All invoices to registered businesses with GSTIN
    /// </summary>
    private async Task<List<B2BInvoice>> GenerateB2BInvoices(List<Invoice> invoices)
    {
        var b2bInvoices = new List<B2BInvoice>();

        foreach (var invoice in invoices.Where(i => !string.IsNullOrEmpty(i.Party?.GSTIN)))
        {
            b2bInvoices.Add(new B2BInvoice
            {
                GSTIN = invoice.Party?.GSTIN ?? string.Empty,
                LegalName = invoice.Party?.Name ?? string.Empty,
                InvoiceNumber = invoice.Number,
                InvoiceDate = invoice.VoucherDate.DateTime,
                InvoiceValue = invoice.TotalAmount,
                PlaceOfSupply = GetPlaceOfSupply(invoice.Party),
                ReverseCharge = false, // TODO: Implement reverse charge logic
                InvoiceType = "Regular",
                TaxableValue = invoice.SubTotal,
                CGSTAmount = invoice.CGSTAmount,
                SGSTAmount = invoice.SGSTAmount,
                IGSTAmount = invoice.IGSTAmount,
                CessAmount = invoice.CessAmount
            });
        }

        return b2bInvoices;
    }

    /// <summary>
    /// Generate B2CL (Business to Consumer Large) invoice details
    /// B2C invoices with value > 2.5 lakh
    /// </summary>
    private async Task<List<B2CLInvoice>> GenerateB2CLInvoices(List<Invoice> invoices)
    {
        var b2clInvoices = new List<B2CLInvoice>();

        foreach (var invoice in invoices.Where(i =>
            string.IsNullOrEmpty(i.Party?.GSTIN) &&
            i.TotalAmount > B2C_LARGE_INVOICE_THRESHOLD))
        {
            b2clInvoices.Add(new B2CLInvoice
            {
                InvoiceNumber = invoice.Number,
                InvoiceDate = invoice.VoucherDate.DateTime,
                InvoiceValue = invoice.TotalAmount,
                PlaceOfSupply = GetPlaceOfSupply(invoice.Party),
                TaxableValue = invoice.SubTotal,
                IGSTAmount = invoice.IGSTAmount, // B2CL is typically inter-state (IGST)
                CessAmount = invoice.CessAmount
            });
        }

        return b2clInvoices;
    }

    /// <summary>
    /// Generate B2CS (Business to Consumer Small) summary
    /// State-wise summary of B2C invoices <= 2.5 lakh
    /// </summary>
    private async Task<List<B2CSInvoice>> GenerateB2CSInvoices(List<Invoice> invoices)
    {
        var b2csInvoices = invoices
            .Where(i => string.IsNullOrEmpty(i.Party?.GSTIN) && i.TotalAmount <= B2C_LARGE_INVOICE_THRESHOLD)
            .GroupBy(i => new
            {
                PlaceOfSupply = GetPlaceOfSupply(i.Party),
                TaxRate = CalculateTaxRate(i)
            })
            .Select(g => new B2CSInvoice
            {
                PlaceOfSupply = g.Key.PlaceOfSupply,
                TaxRate = g.Key.TaxRate,
                TaxableValue = g.Sum(i => i.SubTotal),
                CGSTAmount = g.Sum(i => i.CGSTAmount),
                SGSTAmount = g.Sum(i => i.SGSTAmount),
                IGSTAmount = g.Sum(i => i.IGSTAmount),
                CessAmount = g.Sum(i => i.CessAmount)
            })
            .ToList();

        return b2csInvoices;
    }

    /// <summary>
    /// Generate Export invoice details
    /// </summary>
    private async Task<List<ExportInvoice>> GenerateExportInvoices(List<Invoice> invoices)
    {
        // TODO: Add export invoice identification logic
        // For now, returning empty list
        return new List<ExportInvoice>();
    }

    /// <summary>
    /// Generate HSN-wise summary of outward supplies
    /// </summary>
    public async Task<List<HSNSummary>> GenerateHSNSummary(int? tenantId, DateTime startDate, DateTime endDate)
    {
        var hsnSummary = await context.VoucherLines
            .Include(vl => vl.Voucher)
            .Include(vl => vl.Item)
            .Where(vl =>
                vl.Voucher.TenantId == tenantId &&
                vl.Voucher.VoucherDate >= startDate &&
                vl.Voucher.VoucherDate <= endDate &&
                vl.Voucher.VoucherType == VoucherType.Invoice &&
                vl.Item != null &&
                !string.IsNullOrEmpty(vl.Item.HSNCode))
            .GroupBy(vl => new
            {
                HSNCode = vl.Item!.HSNCode,
                Description = vl.Item.Name,
                UQC = vl.Unit ?? "NOS"
            })
            .Select(g => new HSNSummary
            {
                HSNCode = g.Key.HSNCode ?? string.Empty,
                Description = g.Key.Description ?? string.Empty,
                UQC = g.Key.UQC,
                TotalQuantity = (decimal)g.Sum(vl => vl.Quantity),
                TotalValue = g.Sum(vl => vl.LineTotal),
                TaxableValue = g.Sum(vl => (decimal)(vl.Quantity * (double)vl.UnitPrice) - vl.DiscountAmount),
                CGSTAmount = 0, // Will be calculated from invoice level
                SGSTAmount = 0,
                IGSTAmount = 0,
                CessAmount = 0,
                TaxRate = 0 // TODO: Get from tax code
            })
            .ToListAsync();

        // Calculate tax amounts for each HSN by aggregating from invoices
        foreach (var hsn in hsnSummary)
        {
            var invoicesWithHSN = await context.VoucherLines
                .Include(vl => vl.Voucher)
                .ThenInclude(v => ((Invoice)v))
                .Include(vl => vl.Item)
                .Where(vl =>
                    vl.Voucher.TenantId == tenantId &&
                    vl.Voucher.VoucherDate >= startDate &&
                    vl.Voucher.VoucherDate <= endDate &&
                    vl.Voucher.VoucherType == VoucherType.Invoice &&
                    vl.Item != null &&
                    vl.Item.HSNCode == hsn.HSNCode)
                .Select(vl => vl.Voucher)
                .Cast<Invoice>()
                .Distinct()
                .ToListAsync();

            // Proportional tax allocation based on line value
            foreach (var invoice in invoicesWithHSN)
            {
                var hsnLinesValue = invoice.VoucherLines
                    .Where(vl => vl.Item?.HSNCode == hsn.HSNCode)
                    .Sum(vl => vl.LineTotal);

                var proportion = invoice.TotalAmount > 0 ? hsnLinesValue / invoice.TotalAmount : 0;

                hsn.CGSTAmount += invoice.CGSTAmount * proportion;
                hsn.SGSTAmount += invoice.SGSTAmount * proportion;
                hsn.IGSTAmount += invoice.IGSTAmount * proportion;
                hsn.CessAmount += invoice.CessAmount * proportion;
            }

            // Calculate average tax rate
            if (hsn.TaxableValue > 0)
            {
                var totalTax = hsn.CGSTAmount + hsn.SGSTAmount + hsn.IGSTAmount;
                hsn.TaxRate = (totalTax / hsn.TaxableValue) * 100;
            }
        }

        return hsnSummary;
    }

    #region Helper Methods

    private async Task<List<Invoice>> GetInvoicesForPeriod(int? tenantId, DateTime startDate, DateTime endDate)
    {
        return await context.Invoices
            .Include(i => i.Party)
            .Include(i => i.VoucherLines)
                .ThenInclude(vl => vl.Item)
            .Where(i =>
                i.TenantId == tenantId &&
                i.VoucherDate >= startDate &&
                i.VoucherDate <= endDate &&
                i.IsNotDeleted)
            .ToListAsync();
    }

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

    private async Task<string> GetTenantTradeName(int? tenantId)
    {
        var tenant = await context.TenantAccountingSettings
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        return tenant?.CompanyName ?? string.Empty;
    }

    private string GetPlaceOfSupply(LedgerAccount? party)
    {
        if (party == null) return string.Empty;

        // Return state code - you may need to map state names to codes
        return party.State ?? string.Empty;
    }

    private decimal CalculateTaxRate(Invoice invoice)
    {
        if (invoice.SubTotal == 0) return 0;

        var totalTax = invoice.CGSTAmount + invoice.SGSTAmount + invoice.IGSTAmount;
        return (totalTax / invoice.SubTotal) * 100;
    }

    #endregion
}
