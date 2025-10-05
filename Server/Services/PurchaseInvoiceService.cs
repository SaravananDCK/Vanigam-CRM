using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseInvoiceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PurchaseInvoice>> logger,
    LedgerPostingService ledgerPostingService)
    : BaseService<PurchaseInvoice>(context, logger)
{
    public override DbSet<PurchaseInvoice> GetDbSet()
    {
        return Context.PurchaseInvoices;
    }

    /// <summary>
    /// Creates a new purchase invoice and posts it to the ledger if status is Posted.
    /// </summary>
    protected override async Task OnCreatedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await base.OnCreatedAsync(purchaseInvoice);

            // Post to ledger if purchase invoice is already in Posted status
            if (purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
            {
                await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                await Context.SaveChangesAsync();

                // Validate entries balance
                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                }
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates a purchase invoice. If status changes to Posted, creates ledger entries.
    /// If status changes from Posted to another status, reverses ledger entries.
    /// </summary>
    protected override async Task OnUpdatedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the original purchase invoice to check status changes
            var original = await Context.PurchaseInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Oid == purchaseInvoice.Oid);

            await base.OnUpdatedAsync(purchaseInvoice);

            // Handle status changes
            if (original != null)
            {
                // Status changed from non-Posted to Posted
                if (original.Status != PurchaseInvoiceStatus.Posted && purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                    }
                }
                // Status changed from Posted to non-Posted (reversal)
                else if (original.Status == PurchaseInvoiceStatus.Posted && purchaseInvoice.Status != PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.ReverseVoucherEntries(purchaseInvoice.Oid, $"Purchase Invoice {purchaseInvoice.Number} status changed to {purchaseInvoice.Status}");
                    await Context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Deletes a purchase invoice. If it was posted, reverses the ledger entries first.
    /// </summary>
    protected override async Task<bool> OnDeletedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            if (purchaseInvoice == null)
                return false;

            // Reverse ledger entries if purchase invoice was posted
            if (purchaseInvoice.Status >= PurchaseInvoiceStatus.Posted)
            {
                await ledgerPostingService.ReverseVoucherEntries(purchaseInvoice.Oid, $"Purchase Invoice {purchaseInvoice.Number} deleted");
                await Context.SaveChangesAsync();
            }

            var deleted = await base.DeleteAsync(purchaseInvoice);
            await transaction.CommitAsync();
            return deleted;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
