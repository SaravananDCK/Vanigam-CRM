using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
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
    /// - Debit: Customer Account (Accounts Receivable) - Full invoice amount
    /// - Credit: Sales Account - Gross sales before discount
    /// - Debit: Sales Discount Account (if discount exists) - Reduces revenue
    /// - Credit: SGST Payable Account (if SGST exists)
    /// - Credit: CGST Payable Account (if CGST exists)
    /// - Credit: IGST Payable Account (if IGST exists)
    /// </summary>
    public async Task PostInvoiceToLedger(Invoice invoice)
    {
        try
        {
            logger.LogInformation($"Posting Invoice {invoice.Number} to ledger", invoice.Number);

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

            // Calculate gross sales
            var grossSales = invoice.SubTotal - invoice.DiscountAmount;

            // 1. Debit Customer Account (Asset increases) - Full invoice amount including tax
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

            // 2. Credit Sales Account (Revenue increases) - Gross sales before discount
            entries.Add(new LedgerEntry
            {
                TenantId = invoice.TenantId,
                VoucherId = invoice.Oid,
                AccountId = salesAccount.Oid,
                EntryType = EntryType.Credit,
                Amount = grossSales,
                EntryDate = invoice.VoucherDate,
                EntryNumber = invoice.Number,
                Description = $"Sales Revenue - {invoice.Number}",
                Reference = invoice.Number,
                IsReconciled = false
            });

            // 3. Debit Sales Discount Account (if discount exists) - Contra revenue account
            //if (invoice.DiscountAmount > 0)
            //{
            //    var discountAccount = await GetSalesDiscountAccount(invoice.TenantId);
            //    if (discountAccount != null)
            //    {
            //        entries.Add(new LedgerEntry
            //        {
            //            TenantId = invoice.TenantId,
            //            VoucherId = invoice.Oid,
            //            AccountId = discountAccount.Oid,
            //            EntryType = EntryType.Debit,
            //            Amount = invoice.DiscountAmount,
            //            EntryDate = invoice.VoucherDate,
            //            EntryNumber = invoice.Number,
            //            Description = $"Sales Discount - {invoice.Number}",
            //            Reference = invoice.Number,
            //            IsReconciled = false
            //        });
            //    }
            //    else
            //    {
            //        logger.LogWarning($"Sales discount account not configured for tenant {invoice.TenantId}. Discount amount {invoice.DiscountAmount} not posted separately.",
            //            invoice.TenantId, invoice.DiscountAmount);
            //    }
            //}

            // 4. Credit SGST Payable Account (if SGST exists)
            if (invoice.SGSTAmount > 0)
            {
                var sgstAccount = await GetSGSTPayableAccount(invoice.TenantId);
                if (sgstAccount == null)
                {
                    throw new InvalidOperationException($"SGST payable account not configured for tenant {invoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = invoice.TenantId,
                    VoucherId = invoice.Oid,
                    AccountId = sgstAccount.Oid,
                    EntryType = EntryType.Credit,
                    Amount = invoice.SGSTAmount,
                    EntryDate = invoice.VoucherDate,
                    EntryNumber = invoice.Number,
                    Description = $"SGST Collected - {invoice.Number}",
                    Reference = invoice.Number,
                    IsReconciled = false
                });
            }

            // 5. Credit CGST Payable Account (if CGST exists)
            if (invoice.CGSTAmount > 0)
            {
                var cgstAccount = await GetCGSTPayableAccount(invoice.TenantId);
                if (cgstAccount == null)
                {
                    throw new InvalidOperationException($"CGST payable account not configured for tenant {invoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = invoice.TenantId,
                    VoucherId = invoice.Oid,
                    AccountId = cgstAccount.Oid,
                    EntryType = EntryType.Credit,
                    Amount = invoice.CGSTAmount,
                    EntryDate = invoice.VoucherDate,
                    EntryNumber = invoice.Number,
                    Description = $"CGST Collected - {invoice.Number}",
                    Reference = invoice.Number,
                    IsReconciled = false
                });
            }

            // 6. Credit IGST Payable Account (if IGST exists)
            if (invoice.IGSTAmount > 0)
            {
                var igstAccount = await GetIGSTPayableAccount(invoice.TenantId);
                if (igstAccount == null)
                {
                    throw new InvalidOperationException($"IGST payable account not configured for tenant {invoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = invoice.TenantId,
                    VoucherId = invoice.Oid,
                    AccountId = igstAccount.Oid,
                    EntryType = EntryType.Credit,
                    Amount = invoice.IGSTAmount,
                    EntryDate = invoice.VoucherDate,
                    EntryNumber = invoice.Number,
                    Description = $"IGST Collected - {invoice.Number}",
                    Reference = invoice.Number,
                    IsReconciled = false
                });
            }

            // Fallback: If no GST components but TaxAmount exists (legacy support)
            if (invoice.TaxAmount > 0 && invoice.SGSTAmount == 0 && invoice.CGSTAmount == 0 && invoice.IGSTAmount == 0)
            {
                var taxAccount = await GetDefaultTaxPayableAccount(invoice.TenantId);
                if (taxAccount != null)
                {
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
            }
            await context.LedgerEntries.AddRangeAsync(entries);

            // Validate entries balance
            var totalDebits = Math.Round(entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount));
            var totalCredits = Math.Round(entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount));
            totalDebits = Math.Round(totalDebits);
            totalCredits = Math.Round(totalCredits);

            if (totalDebits != totalCredits)
            {
                logger.LogError($"Invoice {invoice.Number} ledger entries do not balance. Debits: {totalDebits}, Credits: {totalCredits}",
                    invoice.Number, totalDebits, totalCredits);
                throw new InvalidOperationException($"Ledger entries for invoice {invoice.Number} do not balance. Debits: {totalDebits}, Credits: {totalCredits}");
            }

            logger.LogInformation($"Successfully posted {entries.Count} ledger entries for Invoice {invoice.Number}. Debits: {totalDebits}, Credits: {totalCredits}",
                entries.Count, invoice.Number, totalDebits, totalCredits);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error posting Invoice {invoice.Number} to ledger", invoice.Number);
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
            logger.LogInformation($"Posting Payment {payment.Number} to ledger", payment.Number);

            if (!payment.BankAccountId.HasValue)
            {
                throw new InvalidOperationException($"Payment {payment.Number} has no bank account assigned");
            }

            if (!payment.PartyId.HasValue)
            {
                throw new InvalidOperationException($"Payment {payment.Number} has no customer assigned");
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
                EntryNumber = payment.Number,
                Description = $"Payment Received - {payment.Number}",
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
                EntryNumber = payment.Number,
                Description = $"Payment Received - {payment.Number}",
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
    /// - Debit: Purchases/Expense Account - Gross purchases before discount
    /// - Credit: Purchase Discount Account (if discount exists) - Reduces expense
    /// - Debit: SGST Input Account (if SGST exists) - Recoverable input tax
    /// - Debit: CGST Input Account (if CGST exists) - Recoverable input tax
    /// - Debit: IGST Input Account (if IGST exists) - Recoverable input tax
    /// - Credit: Vendor Account (Accounts Payable) - Full invoice amount
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

            // Calculate gross purchases (before discount)
            var grossPurchases = purchaseInvoice.SubTotal - purchaseInvoice.DiscountAmount;

            // 1. Debit Purchases Account (Expense increases) - Gross purchases before discount
            entries.Add(new LedgerEntry
            {
                TenantId = purchaseInvoice.TenantId,
                VoucherId = purchaseInvoice.Oid,
                AccountId = purchasesAccount.Oid,
                EntryType = EntryType.Debit,
                Amount = grossPurchases,
                EntryDate = purchaseInvoice.VoucherDate,
                EntryNumber = purchaseInvoice.Number,
                Description = $"Purchase Invoice - {purchaseInvoice.Number}",
                Reference = purchaseInvoice.Number,
                IsReconciled = false
            });

            // 2. Credit Purchase Discount Account (if discount exists) - Contra expense account
            //if (purchaseInvoice.DiscountAmount > 0)
            //{
            //    var discountAccount = await GetPurchaseDiscountAccount(purchaseInvoice.TenantId);
            //    if (discountAccount != null)
            //    {
            //        entries.Add(new LedgerEntry
            //        {
            //            TenantId = purchaseInvoice.TenantId,
            //            VoucherId = purchaseInvoice.Oid,
            //            AccountId = discountAccount.Oid,
            //            EntryType = EntryType.Credit,
            //            Amount = purchaseInvoice.DiscountAmount,
            //            EntryDate = purchaseInvoice.VoucherDate,
            //            EntryNumber = purchaseInvoice.Number,
            //            Description = $"Purchase Discount - {purchaseInvoice.Number}",
            //            Reference = purchaseInvoice.Number,
            //            IsReconciled = false
            //        });
            //    }
            //    else
            //    {
            //        logger.LogWarning("Purchase discount account not configured for tenant {TenantId}. Discount amount {DiscountAmount} not posted separately.",
            //            purchaseInvoice.TenantId, purchaseInvoice.DiscountAmount);
            //    }
            //}

            // 3. Debit SGST Input Account (if SGST exists) - Input tax credit
            if (purchaseInvoice.SGSTAmount > 0)
            {
                var sgstInputAccount = await GetSGSTInputAccount(purchaseInvoice.TenantId);
                if (sgstInputAccount == null)
                {
                    throw new InvalidOperationException($"SGST input account not configured for tenant {purchaseInvoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = purchaseInvoice.TenantId,
                    VoucherId = purchaseInvoice.Oid,
                    AccountId = sgstInputAccount.Oid,
                    EntryType = EntryType.Debit,
                    Amount = purchaseInvoice.SGSTAmount,
                    EntryDate = purchaseInvoice.VoucherDate,
                    EntryNumber = purchaseInvoice.Number,
                    Description = $"SGST Input - {purchaseInvoice.Number}",
                    Reference = purchaseInvoice.Number,
                    IsReconciled = false
                });
            }

            // 4. Debit CGST Input Account (if CGST exists) - Input tax credit
            if (purchaseInvoice.CGSTAmount > 0)
            {
                var cgstInputAccount = await GetCGSTInputAccount(purchaseInvoice.TenantId);
                if (cgstInputAccount == null)
                {
                    throw new InvalidOperationException($"CGST input account not configured for tenant {purchaseInvoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = purchaseInvoice.TenantId,
                    VoucherId = purchaseInvoice.Oid,
                    AccountId = cgstInputAccount.Oid,
                    EntryType = EntryType.Debit,
                    Amount = purchaseInvoice.CGSTAmount,
                    EntryDate = purchaseInvoice.VoucherDate,
                    EntryNumber = purchaseInvoice.Number,
                    Description = $"CGST Input - {purchaseInvoice.Number}",
                    Reference = purchaseInvoice.Number,
                    IsReconciled = false
                });
            }

            // 5. Debit IGST Input Account (if IGST exists) - Input tax credit
            if (purchaseInvoice.IGSTAmount > 0)
            {
                var igstInputAccount = await GetIGSTInputAccount(purchaseInvoice.TenantId);
                if (igstInputAccount == null)
                {
                    throw new InvalidOperationException($"IGST input account not configured for tenant {purchaseInvoice.TenantId}");
                }

                entries.Add(new LedgerEntry
                {
                    TenantId = purchaseInvoice.TenantId,
                    VoucherId = purchaseInvoice.Oid,
                    AccountId = igstInputAccount.Oid,
                    EntryType = EntryType.Debit,
                    Amount = purchaseInvoice.IGSTAmount,
                    EntryDate = purchaseInvoice.VoucherDate,
                    EntryNumber = purchaseInvoice.Number,
                    Description = $"IGST Input - {purchaseInvoice.Number}",
                    Reference = purchaseInvoice.Number,
                    IsReconciled = false
                });
            }

            // Fallback: If no GST components but TaxAmount exists (legacy support)
            if (purchaseInvoice.TaxAmount > 0 && purchaseInvoice.SGSTAmount == 0 && purchaseInvoice.CGSTAmount == 0 && purchaseInvoice.IGSTAmount == 0)
            {
                var taxInputAccount = await GetDefaultTaxInputAccount(purchaseInvoice.TenantId);
                if (taxInputAccount != null)
                {
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
            }

            // 6. Credit Vendor Account (Liability increases) - Full invoice amount
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

            // Validate entries balance
            var totalDebits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            totalDebits = Math.Round(totalDebits);
            totalCredits = Math.Round(totalCredits);

            if (totalDebits != totalCredits)
            {
                logger.LogError($"Purchase Invoice {purchaseInvoice.Number} ledger entries do not balance. Debits: {totalDebits}, Credits: {totalCredits}",
                    purchaseInvoice.Number, totalDebits, totalCredits);
                throw new InvalidOperationException($"Ledger entries for purchase invoice {purchaseInvoice.Number} do not balance. Debits: {totalDebits}, Credits: {totalCredits}");
            }

            logger.LogInformation($"Successfully posted {entries.Count} ledger entries for Purchase Invoice {purchaseInvoice.Number}. Debits: {totalDebits}, Credits: {totalCredits}",
                entries.Count, purchaseInvoice.Number, totalDebits, totalCredits);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error posting Purchase Invoice {purchaseInvoice.Number} to ledger", purchaseInvoice.Number);
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
    /// Gets the tenant accounting settings with all navigation properties loaded.
    /// </summary>
    private async Task<TenantAccountingSettings?> GetTenantAccountingSettings(int? tenantId)
    {
        var settings = await context.TenantAccountingSettings
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
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsNotDeleted && s.IsActive);

        return settings;
    }

    private async Task<LedgerAccount?> GetDefaultSalesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultSalesAccount != null)
            return settings.DefaultSalesAccount;

        // Fallback: Look for sales account by code (from LedgerAccountSeedData.json)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "SALES"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultPurchasesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultPurchasesAccount != null)
            return settings.DefaultPurchasesAccount;

        // Fallback: Look for purchases account by code (from LedgerAccountSeedData.json)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "PURCHASES"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultTaxPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultTaxPayableAccount != null)
            return settings.DefaultTaxPayableAccount;

        // Fallback: Try SGST as general tax payable (most common for intra-state)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX001" // SGST Payable
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultTaxInputAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultTaxInputAccount != null)
            return settings.DefaultTaxInputAccount;

        // Fallback: Try SGST Input as general tax input (most common for intra-state)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX004" // SGST Input
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultWIPAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.WorkInProgressAccount != null)
            return settings.WorkInProgressAccount;

        // Fallback: Look for WIP account by code (not in default seed data)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code == "WIP" || a.Code == "WIP001")
                && a.IsActive
                && a.IsNotDeleted);
    }

    // GST-specific Account Helpers

    private async Task<LedgerAccount?> GetSGSTPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultSGSTPayableAccount != null)
            return settings.DefaultSGSTPayableAccount;

        // Fallback: TAX001 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX001"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetCGSTPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultCGSTPayableAccount != null)
            return settings.DefaultCGSTPayableAccount;

        // Fallback: TAX002 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX002"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetIGSTPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultIGSTPayableAccount != null)
            return settings.DefaultIGSTPayableAccount;

        // Fallback: TAX003 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX003"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetSGSTInputAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultSGSTInputAccount != null)
            return settings.DefaultSGSTInputAccount;

        // Fallback: TAX004 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX004"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetCGSTInputAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultCGSTInputAccount != null)
            return settings.DefaultCGSTInputAccount;

        // Fallback: TAX005 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX005"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetIGSTInputAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultIGSTInputAccount != null)
            return settings.DefaultIGSTInputAccount;

        // Fallback: TAX006 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "TAX006"
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Payment Method Account Helpers

    private async Task<LedgerAccount?> GetDefaultCashAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultCashAccount != null)
            return settings.DefaultCashAccount;

        // Fallback: CASH001 from LedgerAccountSeedData.json
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code == "CASH001"
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultBankAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultBankAccount != null)
            return settings.DefaultBankAccount;

        // Fallback: Not in default seed data, look for BANK prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code.StartsWith("BANK")
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultCardAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultCardAccount != null)
            return settings.DefaultCardAccount;

        // Fallback: Not in default seed data, look for CARD prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code.StartsWith("CARD")
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultUpiAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultUpiAccount != null)
            return settings.DefaultUpiAccount;

        // Fallback: Not in default seed data, look for UPI prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code.StartsWith("UPI")
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Receivables and Payables Account Helpers

    private async Task<LedgerAccount?> GetDefaultReceivableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultReceivableAccount != null)
            return settings.DefaultReceivableAccount;

        // Fallback: Not in default seed data, look for AR or RECV prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("AR") || a.Code.StartsWith("RECV"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetDefaultPayableAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultPayableAccount != null)
            return settings.DefaultPayableAccount;

        // Fallback: Not in default seed data, look for AP or PAYABLE prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("AP") || a.Code.StartsWith("PAYABLE"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Inventory and COGS Account Helpers

    private async Task<LedgerAccount?> GetDefaultInventoryAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.DefaultInventoryAccount != null)
            return settings.DefaultInventoryAccount;

        // Fallback: STK001 from LedgerAccountSeedData.json (Stock in Hand)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code == "STK001" || a.Code.StartsWith("STK"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetCostOfGoodsSoldAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.CostOfGoodsSoldAccount != null)
            return settings.CostOfGoodsSoldAccount;

        // Fallback: Not in default seed data, look for COGS prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.Code.StartsWith("COGS")
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Discount and Adjustment Account Helpers

    private async Task<LedgerAccount?> GetSalesDiscountAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.SalesDiscountAccount != null)
            return settings.SalesDiscountAccount;

        // Fallback: Not in default seed data, look for DISC prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("DISC") || a.Code.StartsWith("SAL") && a.Name.Contains("Discount"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetPurchaseDiscountAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.PurchaseDiscountAccount != null)
            return settings.PurchaseDiscountAccount;

        // Fallback: Not in default seed data, look for DISC prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("PDISC") || a.Code.StartsWith("PUR") && a.Name.Contains("Discount"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetRoundingAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.RoundingAccount != null)
            return settings.RoundingAccount;

        // Fallback: Not in default seed data, look for ROUND prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("ROUND") || a.Name.Contains("Rounding"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetExchangeGainLossAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.ExchangeGainLossAccount != null)
            return settings.ExchangeGainLossAccount;

        // Fallback: Not in default seed data, look for EXCH prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("EXCH") || a.Name.Contains("Exchange"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Freight and Charges Account Helpers

    private async Task<LedgerAccount?> GetFreightChargesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.FreightChargesAccount != null)
            return settings.FreightChargesAccount;

        // Fallback: Not in default seed data, look for FREIGHT or EXP prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("FREIGHT") || a.Code.StartsWith("EXP") && a.Name.Contains("Freight"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetPackingChargesAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.PackingChargesAccount != null)
            return settings.PackingChargesAccount;

        // Fallback: Not in default seed data, look for PACK or EXP prefix
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code.StartsWith("PACK") || a.Code.StartsWith("EXP") && a.Name.Contains("Packing"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    // Return Account Helpers

    private async Task<LedgerAccount?> GetSalesReturnAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.SalesReturnAccount != null)
            return settings.SalesReturnAccount;

        // Fallback: SAL002 from LedgerAccountSeedData.json (Sales Return)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code == "SAL002" || a.Code.StartsWith("SAL") && a.Name.Contains("Return"))
                && a.IsActive
                && a.IsNotDeleted);
    }

    private async Task<LedgerAccount?> GetPurchaseReturnAccount(int? tenantId)
    {
        var settings = await GetTenantAccountingSettings(tenantId);
        if (settings?.PurchaseReturnAccount != null)
            return settings.PurchaseReturnAccount;

        // Fallback: PUR002 from LedgerAccountSeedData.json (Purchase Return)
        return await context.LedgerAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && (a.Code == "PUR002" || a.Code.StartsWith("PUR") && a.Name.Contains("Return"))
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
        totalDebits = Math.Round(totalDebits);
        var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
        totalCredits = Math.Round(totalCredits);

        var isBalanced = totalDebits == totalCredits;

        if (!isBalanced)
        {
            logger.LogWarning($"Voucher {voucherId} entries are not balanced. Debits: {totalDebits}, Credits: {totalCredits}",
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
        totalDebits = Math.Round(totalDebits);
        var totalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
        totalCredits = Math.Round(totalCredits);

        return (totalDebits, totalCredits, totalDebits == totalCredits);
    }

    #endregion
}
