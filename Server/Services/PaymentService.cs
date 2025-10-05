using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PaymentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Payment>> logger,
    LedgerPostingService ledgerPostingService)
    : BaseService<Payment>(context, logger)
{
    public override DbSet<Payment> GetDbSet()
    {
        return Context.Payments;
    }

    /// <summary>
    /// Creates a new payment and posts it to the ledger.
    /// </summary>
    protected override async Task OnCreatedAsync(Payment payment)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await base.OnCreatedAsync(payment);

            // Always post payments to ledger if they have allocated amount
            if (payment.AllocatedAmount > 0)
            {
                await ledgerPostingService.PostPaymentToLedger(payment);
                await Context.SaveChangesAsync();

                // Validate entries balance
                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(payment.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Payment {payment.ReferenceNumber} are not balanced");
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
    /// Updates a payment. Reverses old entries and creates new ones.
    /// </summary>
    protected override async Task OnUpdatedAsync(Payment payment)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check if payment has existing ledger entries
            var hasExistingEntries = await Context.LedgerEntries
                .AnyAsync(e => e.VoucherId == payment.Oid && e.IsNotDeleted);

            if (hasExistingEntries)
            {
                // Reverse existing entries
                await ledgerPostingService.ReverseVoucherEntries(payment.Oid, $"Payment {payment.ReferenceNumber} updated");
            }

            await base.OnUpdatedAsync(payment);

            // Post new entries if there's allocated amount
            if (payment.AllocatedAmount > 0)
            {
                await ledgerPostingService.PostPaymentToLedger(payment);
                await Context.SaveChangesAsync();

                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(payment.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Payment {payment.ReferenceNumber} are not balanced");
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
    /// Deletes a payment and reverses its ledger entries.
    /// </summary>
    protected override async Task<bool> OnDeletedAsync(Payment payment)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            if (payment == null)
                return false;

            // Reverse ledger entries if they exist
            var hasEntries = await Context.LedgerEntries
                .AnyAsync(e => e.VoucherId == payment.Oid && e.IsNotDeleted);

            if (hasEntries)
            {
                await ledgerPostingService.ReverseVoucherEntries(payment.Oid, $"Payment {payment.ReferenceNumber} deleted");
                await Context.SaveChangesAsync();
            }

            var deleted = await base.DeleteAsync(payment);
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