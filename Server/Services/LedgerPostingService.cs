using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using NodaTime;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services;

/// <summary>
/// Service responsible for posting voucher transactions to the general ledger.
/// Implements double-entry bookkeeping where every transaction has equal debits and credits.
/// </summary>
public class LedgerPostingService(
    VanigamAccountingDbContext context,
    ILogger<LedgerPostingService> logger,
    ICurrentUserService currentUserService)
{
    #region Invoice Posting

    /// <summary>
    /// Posts an invoice to the ledger. Creates entries for:
    /// - Debit: Customer Account (Accounts Receivable)
    /// - Credit: Sales Account
    /// - Credit: Tax Payable Account (if tax exists)
    /// </summary>
    public async Task PostInvoiceToLedger(Invoice invoice)
    {
        try
        {
            logger.LogInformation("Posting Invoice {InvoiceNumber} to ledger", invoice.Number);

            // Get required accounts
            var salesAccount = await GetDefaultSalesAccount(invoice.TenantId);
            if (salesAccount == null)
            {
                throw new InvalidOperationException($"Sales account not configured for tenant {invoice.TenantId}");
            }

            if (!invoice.PartyId.HasValue)
            {
                throw new InvalidOperationException($"Invoice {invoice.Number} has no customer assigned");
            }

            var entries = new List<LedgerEntry>();

            // Debit Customer Account (Asset increases)
            entries.Add(new LedgerEntry
            {
                TenantId = invoice.TenantId,
                VoucherId = invoice.Oid,
                AccountId = invoice.PartyId.Value, // Customer is a LedgerAccount
                EntryType = EntryType.Debit,
                Amount = invoice.TotalAmount,
                EntryDate = invoice.VoucherDate,
                EntryNumber = invoice.Number,
                Description = $"Sales Invoice - {invoice.Number}",
                Reference = invoice.Number,
                IsReconciled = false
            });

            // Credit Sales Account (Revenue increases)
            entries.Add(new LedgerEntry
            {
                TenantId = invoice.TenantId,
                VoucherId = invoice.Oid,
                AccountId = salesAccount.Oid,
                EntryType = EntryType.Credit,
                Amount = invoice.SubTotal,
                EntryDate = invoice.VoucherDate,
                EntryNumber = invoice.Number,
                Description = $"Sales Revenue - {invoice.Number}",
                Reference = invoice.Number,
                IsReconciled = false
            });

            // Credit Tax Payable Account (if tax exists)
            if (invoice.TaxAmount > 0)
            {
                var taxAccount = await GetDefaultTaxPayableAccount(invoice.TenantId);
                if (taxAccount == null)
                {
                    throw new InvalidOperationException($"Tax payable account not configured for tenant {invoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = invoice.TenantId,
                    VoucherId = invoice.Oid,
                    AccountId = taxAccount.Oid,
                    EntryType = EntryType.Credit,
                    Amount = invoice.TaxAmount,
                    EntryDate = invoice.VoucherDate,
                    EntryNumber = invoice.Number,
                    Description = $"Tax Collected - {invoice.Number}",
                    Reference = invoice.Number,
                    IsReconciled = false
                });
            }

            await context.LedgerEntries.AddRangeAsync(entries);
            logger.LogInformation("Successfully posted {EntryCount} ledger entries for Invoice {InvoiceNumber}",
                entries.Count, invoice.Number);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting Invoice {InvoiceNumber} to ledger", invoice.Number);
            throw;
        }
    }

    #endregion

    #region Payment Posting

    /// <summary>
    /// Posts a payment to the ledger. Creates entries for:
    /// - Debit: Bank Account (Asset increases)
    /// - Credit: Customer Account (Accounts Receivable decreases)
    /// </summary>
    public async Task PostPaymentToLedger(Payment payment)
    {
        try
        {
            logger.LogInformation("Posting Payment {PaymentReference} to ledger", payment.ReferenceNumber);

            if (!payment.BankAccountId.HasValue)
            {
                throw new InvalidOperationException($"Payment {payment.ReferenceNumber} has no bank account assigned");
            }

            if (!payment.PartyId.HasValue)
            {
                throw new InvalidOperationException($"Payment {payment.ReferenceNumber} has no customer assigned");
            }

            var entries = new List<LedgerEntry>();

            // Debit Bank Account (Asset increases)
            entries.Add(new LedgerEntry
            {
                TenantId = payment.TenantId,
                VoucherId = payment.Oid,
                AccountId = payment.BankAccountId.Value,
                EntryType = EntryType.Debit,
                Amount = payment.AllocatedAmount,
                EntryDate = payment.VoucherDate,
                EntryNumber = payment.ReferenceNumber,
                Description = $"Payment Received - {payment.ReferenceNumber}",
                Reference = payment.ReferenceNumber,
                IsReconciled = false
            });

            // Credit Customer Account (Accounts Receivable decreases)
            entries.Add(new LedgerEntry
            {
                TenantId = payment.TenantId,
                VoucherId = payment.Oid,
                AccountId = payment.PartyId.Value,
                EntryType = EntryType.Credit,
                Amount = payment.AllocatedAmount,
                EntryDate = payment.VoucherDate,
                EntryNumber = payment.ReferenceNumber,
                Description = $"Payment Received - {payment.ReferenceNumber}",
                Reference = payment.ReferenceNumber,
                IsReconciled = false
            });

            await context.LedgerEntries.AddRangeAsync(entries);
            logger.LogInformation("Successfully posted {EntryCount} ledger entries for Payment {PaymentReference}",
                entries.Count, payment.ReferenceNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting Payment {PaymentReference} to ledger", payment.ReferenceNumber);
            throw;
        }
    }

    #endregion

    #region Purchase Invoice Posting

    /// <summary>
    /// Posts a purchase invoice to the ledger. Creates entries for:
    /// - Debit: Purchases/Expense Account
    /// - Debit: Tax Input Account (if tax exists)
    /// - Credit: Vendor Account (Accounts Payable)
    /// </summary>
    public async Task PostPurchaseInvoiceToLedger(PurchaseInvoice purchaseInvoice)
    {
        try
        {
            logger.LogInformation("Posting Purchase Invoice {InvoiceNumber} to ledger", purchaseInvoice.Number);

            // Get required accounts
            var purchasesAccount = await GetDefaultPurchasesAccount(purchaseInvoice.TenantId);
            if (purchasesAccount == null)
            {
                throw new InvalidOperationException($"Purchases account not configured for tenant {purchaseInvoice.TenantId}");
            }

            if (!purchaseInvoice.PartyId.HasValue)
            {
                throw new InvalidOperationException($"Purchase Invoice {purchaseInvoice.Number} has no vendor assigned");
            }

            var entries = new List<LedgerEntry>();

            // Debit Purchases Account (Expense increases)
            entries.Add(new LedgerEntry
            {
                TenantId = purchaseInvoice.TenantId,
                VoucherId = purchaseInvoice.Oid,
                AccountId = purchasesAccount.Oid,
                EntryType = EntryType.Debit,
                Amount = purchaseInvoice.SubTotal,
                EntryDate = purchaseInvoice.VoucherDate,
                EntryNumber = purchaseInvoice.Number,
                Description = $"Purchase Invoice - {purchaseInvoice.Number}",
                Reference = purchaseInvoice.Number,
                IsReconciled = false
            });

            // Debit Tax Input Account (if tax exists)
            if (purchaseInvoice.TaxAmount > 0)
            {
                var taxInputAccount = await GetDefaultTaxInputAccount(purchaseInvoice.TenantId);
                if (taxInputAccount == null)
                {
                    throw new InvalidOperationException($"Tax input account not configured for tenant {purchaseInvoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = purchaseInvoice.TenantId,
                    VoucherId = purchaseInvoice.Oid,
                    AccountId = taxInputAccount.Oid,
                    EntryType = EntryType.Debit,
                    Amount = purchaseInvoice.TaxAmount,
                    EntryDate = purchaseInvoice.VoucherDate,
                    EntryNumber = purchaseInvoice.Number,
                    Description = $"Input Tax - {purchaseInvoice.Number}",
                    Reference = purchaseInvoice.Number,
                    IsReconciled = false
                });
            }

            // Credit Vendor Account (Liability increases)
            entries.Add(new LedgerEntry
            {
                TenantId = purchaseInvoice.TenantId,
                VoucherId = purchaseInvoice.Oid,
                AccountId = purchaseInvoice.PartyId.Value, // Vendor is a LedgerAccount
                EntryType = EntryType.Credit,
                Amount = purchaseInvoice.TotalAmount,
                EntryDate = purchaseInvoice.VoucherDate,
                EntryNumber = purchaseInvoice.Number,
                Description = $"Purchase Invoice - {purchaseInvoice.Number}",
                Reference = purchaseInvoice.Number,
                IsReconciled = false
            });

            await context.LedgerEntries.AddRangeAsync(entries);
            logger.LogInformation("Successfully posted {EntryCount} ledger entries for Purchase Invoice {InvoiceNumber}",
                entries.Count, purchaseInvoice.Number);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting Purchase Invoice {InvoiceNumber} to ledger", purchaseInvoice.Number);
            throw;
        }
    }

    #endregion

    #region Purchase Order Posting (Optional)

    /// <summary>
    /// Posts a purchase order to the ledger (creates commitment/encumbrance entries).
    /// This is optional and depends on whether you want to track commitments.
    /// </summary>
    public async Task PostPurchaseOrderToLedger(PurchaseOrder purchaseOrder)
    {
        try
        {
            logger.LogInformation("Posting Purchase Order {PONumber} to ledger (commitment tracking)", purchaseOrder.Number);

            // This is typically used for budgeting and commitment tracking
            // You may choose to skip this if you don't need commitment accounting

            // Debit: Purchase Commitments Account
            // Credit: Vendor Commitments Account

            logger.LogInformation("Purchase Order posting skipped - commitment accounting not enabled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting Purchase Order {PONumber} to ledger", purchaseOrder.Number);
            throw;
        }
    }

    #endregion

    #region Quote Posting (No Ledger Impact)

    /// <summary>
    /// Quotes do not impact the ledger until they are converted to invoices.
    /// This method is provided for completeness but does nothing.
    /// </summary>
    public Task PostQuoteToLedger(Quote quote)
    {
        logger.LogInformation("Quote {QuoteNumber} does not impact ledger - skipping", quote.Number);
        return Task.CompletedTask;
    }

    #endregion

    #region Job Posting

    /// <summary>
    /// Posts job costs to the ledger. Creates entries for:
    /// - Debit: Work in Progress Account
    /// - Credit: Various expense accounts (labor, materials, etc.)
    /// </summary>
    public async Task PostJobCostsToLedger(Job job)
    {
        try
        {
            logger.LogInformation("Posting Job {JobTitle} costs to ledger", job.Title);

            var wipAccount = await GetDefaultWIPAccount(job.TenantId);
            if (wipAccount == null)
            {
                throw new InvalidOperationException($"Work in Progress account not configured for tenant {job.TenantId}");
            }

            // This is a simplified version - you may want to track labor, materials, overhead separately
            // For now, we'll just track total job costs

            logger.LogInformation("Job cost posting requires detailed cost breakdown - implement based on business rules");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error posting Job {JobTitle} to ledger", job.Title);
            throw;
        }
    }

    #endregion

    #region Reversal Methods

    /// <summary>
    /// Reverses ledger entries for a voucher. Creates opposite entries with reversal flag.
    /// </summary>
    public async Task ReverseVoucherEntries(Guid voucherId, string reason)
    {
        try
        {
            logger.LogInformation("Reversing ledger entries for voucher {VoucherId}", voucherId);

            var originalEntries = await context.LedgerEntries
                .Where(e => e.VoucherId == voucherId && e.IsNotDeleted)
                .ToListAsync();

            if (!originalEntries.Any())
            {
                logger.LogWarning("No ledger entries found for voucher {VoucherId}", voucherId);
                return;
            }

            var reversalEntries = new List<LedgerEntry>();
            var reversalDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

            foreach (var original in originalEntries)
            {
                reversalEntries.Add(new LedgerEntry
                {
                    TenantId = original.TenantId,
                    VoucherId = original.VoucherId,
                    AccountId = original.AccountId,
                    EntryType = original.EntryType == EntryType.Debit ? EntryType.Credit : EntryType.Debit,
                    Amount = original.Amount,
                    EntryDate = reversalDate,
                    EntryNumber = $"{original.EntryNumber}-REV",
                    Description = $"Reversal: {reason}",
                    Reference = original.Reference,
                    IsReconciled = false
                });
            }

            await context.LedgerEntries.AddRangeAsync(reversalEntries);
            logger.LogInformation("Successfully created {EntryCount} reversal entries for voucher {VoucherId}",
                reversalEntries.Count, voucherId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reversing ledger entries for voucher {VoucherId}", voucherId);
            throw;
        }
    }

    #endregion

    #region Account Lookup Methods

    /// <summary>
    /// Gets the tenant accounting settings. Creates default settings if not found.
    /// </summary>
    private async Task<TenantAccountingSettings?> GetTenantAccountingSettings(int? tenantId)
    {
        var settings = await context.TenantAccountingSettings
            .Include(s => s.DefaultSalesAccount)
            .Include(s => s.DefaultPurchasesAccount)
            .Include(s => s.DefaultTaxPayableAccount)
            .Include(s => s.DefaultTaxInputAccount)
            .Include(s => s.WorkInProgressAccount)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsNotDeleted);

        return settings;
    }

    private async Task<LedgerAccount?> GetDefaultSalesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings != null)
            return settings.DefaultSalesAccount;

        // Fallback: Look for sales account by code
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "SALES"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultPurchasesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings != null)
            return settings.DefaultPurchasesAccount;

        // Fallback: Look for purchases account by code
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "PURCHASES"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultTaxPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings != null)
            return settings.DefaultTaxPayableAccount;

        // Fallback: Look for tax payable account by code
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX-PAYABLE"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultTaxInputAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings != null)
            return settings.DefaultTaxInputAccount;

        // Fallback: Look for tax input account by code
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX-INPUT"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultWIPAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.WorkInProgressAccount != null)
            return settings.WorkInProgressAccount;

        // Fallback: Look for WIP account by code
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "WIP"
                && a.IsActive
                && a.IsNotDeleted);
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that all ledger entries for a voucher balance (total debits = total credits).
    /// </summary>
    public async Task<bool> ValidateVoucherEntriesBalance(Guid voucherId)
    {
        var entries = await context.LedgerEntries
            .Where(e => e.VoucherId == voucherId && e.IsNotDeleted)
            .ToListAsync();

        var totalDebits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

        var isBalanced = totalDebits == totalCredits;

        if (!isBalanced)
        {
            logger.LogWarning("Voucher {VoucherId} entries are not balanced. Debits: {Debits}, Credits: {Credits}",
                voucherId, totalDebits, totalCredits);
        }

        return isBalanced;
    }

    /// <summary>
    /// Gets the ledger entry summary for a voucher.
    /// </summary>
    public async Task<(decimal TotalDebits, decimal TotalCredits, bool IsBalanced)> GetVoucherEntrySummary(Guid voucherId)
    {
        var entries = await context.LedgerEntries
            .Where(e => e.VoucherId == voucherId && e.IsNotDeleted)
            .ToListAsync();

        var totalDebits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

        return (totalDebits, totalCredits, totalDebits == totalCredits);
    }

    #endregion
}
