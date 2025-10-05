using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InvoiceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Invoice>> logger,
    LedgerPostingService ledgerPostingService)
    : BaseService<Invoice>(context, logger)
{
    public override DbSet<Invoice> GetDbSet()
    {
        return Context.Invoices;
    }

    /// <summary>
    /// Creates a new invoice and posts it to the ledger if status is Posted.
    /// </summary>
    protected override async Task OnCreatedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await base.OnCreatedAsync(invoice);

            // Post to ledger if invoice is already in Posted status
            if (invoice.Status >= InvoiceStatus.Posted)
            {
                await ledgerPostingService.PostInvoiceToLedger(invoice);
                await Context.SaveChangesAsync();

                // Validate entries balance
                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
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
    /// Updates an invoice. If status changes to Posted, creates ledger entries.
    /// If status changes from Posted to another status, reverses ledger entries.
    /// </summary>
    protected override async Task OnUpdatedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the original invoice to check status changes
            var original = await Context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Oid == invoice.Oid);

            await base.OnUpdatedAsync(invoice);

            // Handle status changes
            if (original != null)
            {
                // Status changed from non-Posted to Posted
                if (original.Status != InvoiceStatus.Posted && invoice.Status >= InvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostInvoiceToLedger(invoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
                    }
                }
                // Status changed from Posted to non-Posted (reversal)
                else if (original.Status >= InvoiceStatus.Posted && invoice.Status < InvoiceStatus.Posted)
                {
                    await ledgerPostingService.ReverseVoucherEntries(invoice.Oid, $"Invoice {invoice.Number} status changed to {invoice.Status}");
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
    /// Deletes an invoice. If it was posted, reverses the ledger entries first.
    /// </summary>
    protected override async Task<bool> OnDeletedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            if (invoice == null)
                return false;

            // Reverse ledger entries if invoice was posted
            if (invoice.Status >= InvoiceStatus.Posted)
            {
                await ledgerPostingService.ReverseVoucherEntries(invoice.Oid, $"Invoice {invoice.Number} deleted");
                await Context.SaveChangesAsync();
            }

            var deleted = await base.DeleteAsync(invoice);
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