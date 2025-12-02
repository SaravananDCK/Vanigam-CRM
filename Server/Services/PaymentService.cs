using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PaymentService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
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

    /// <summary>
    /// Bulk save payment with allocations. Handles create/update of payment and its allocations,
    /// and automatically posts to ledger if payment has allocated amount.
    /// </summary>
    public async Task<Payment> BulkSavePaymentWithAllocations(PaymentBulkSaveDTO paymentData, string currentUserId)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Payment payment;
            bool isUpdate = paymentData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing payment
                payment = await Context.Payments
                    .Include(p => p.Applications)
                    .FirstOrDefaultAsync(p => p.Oid == paymentData.Oid.Value);

                if (payment == null)
                    throw new InvalidOperationException("Payment not found");

                // Update payment properties
                payment.PartyId = paymentData.PartyId;
                payment.PaymentAmount = paymentData.PaymentAmount;
                payment.PaidAt = paymentData.PaidAt;
                payment.VoucherDate = paymentData.VoucherDate;
                payment.PaymentMethod = paymentData.PaymentMethod;
                payment.ReferenceNumber = paymentData.ReferenceNumber;
                payment.BankAccountId = paymentData.BankAccountId;
                payment.Status = paymentData.Status;
                payment.AllocatedAmount = paymentData.AllocatedAmount;
                payment.UnallocatedAmount = paymentData.UnallocatedAmount;

                // Handle payment allocations
                await HandlePaymentAllocations(payment, paymentData.Allocations, currentUserId);

                // Call base update without lifecycle hooks to avoid nested transactions
                Context.Payments.Update(payment);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting
                // Check if payment has existing ledger entries
                var hasExistingEntries = await Context.LedgerEntries
                    .AnyAsync(e => e.VoucherId == payment.Oid && e.IsNotDeleted);

                if (hasExistingEntries)
                {
                    // Reverse existing entries
                    await ledgerPostingService.ReverseVoucherEntries(payment.Oid, $"Payment {payment.ReferenceNumber} updated");
                }

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
            }
            else
            {
                var paymentNumber = await numberSeriesService.GenerateNextNumber(nameof(Payment), TenantId);
                // Create new payment
                payment = new Payment
                {
                    Oid = Guid.NewGuid(),
                    Number = paymentNumber,
                    PartyId = paymentData.PartyId,
                    PaymentAmount = paymentData.PaymentAmount,
                    VoucherDate = paymentData.VoucherDate,
                    PaidAt = paymentData.PaidAt,
                    PaymentMethod = paymentData.PaymentMethod,
                    ReferenceNumber = paymentData.ReferenceNumber,
                    BankAccountId = paymentData.BankAccountId,
                    Status = paymentData.Status,
                    AllocatedAmount = paymentData.AllocatedAmount,
                    UnallocatedAmount = paymentData.UnallocatedAmount,
                    TenantId = TenantId,
                    CreatedByUserId = currentUserId,
                    CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    UpdatedByUserId = currentUserId,
                    UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    IsNotDeleted = true
                };

                // Call base create without lifecycle hooks to avoid nested transactions
                Context.Payments.Add(payment);
                await Context.SaveChangesAsync();

                // Handle payment allocations
                await HandlePaymentAllocations(payment, paymentData.Allocations, currentUserId);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting
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
            }

            // Create customer advance for unallocated amount if any
            if (paymentData.UnallocatedAmount > 0)
            {
                var advance = new CustomerAdvance
                {
                    Oid = Guid.NewGuid(),
                    PaymentId = payment.Oid,
                    Amount = paymentData.UnallocatedAmount,
                    BalanceAmount = paymentData.UnallocatedAmount,
                    AppliedDate = payment.VoucherDate,
                    Reason = "Overpayment / Advance Payment",
                    IsAvailableForAllocation = true,
                    TenantId = TenantId,
                    CreatedByUserId = currentUserId,
                    CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    UpdatedByUserId = currentUserId,
                    UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    IsNotDeleted = true
                };

                Context.CustomerAdvances.Add(advance);
                await Context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload payment with applications
            var savedPayment = await Context.Payments
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Oid == payment.Oid);

            return savedPayment!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles adding payment allocations and updating invoice balances
    /// </summary>
    private async Task HandlePaymentAllocations(Payment payment, List<PaymentAllocationDTO> allocations, string currentUserId)
    {
        // Remove existing allocations
        if (payment.Oid != Guid.Empty)
        {
            var existingAllocations = await Context.PaymentAllocations
                .Where(a => a.PaymentId == payment.Oid)
                .ToListAsync();

            if (existingAllocations.Any())
            {
                Context.PaymentAllocations.RemoveRange(existingAllocations);
            }
        }

        // Add new allocations
        foreach (var allocDto in allocations.Where(a => !a.IsDeleted))
        {
            // Get invoice to update balances
            var invoice = await Context.Invoices.FindAsync(allocDto.InvoiceId);
            if (invoice == null) continue;

            var allocation = new PaymentAllocation
            {
                Oid = Guid.NewGuid(),
                PaymentId = payment.Oid,
                InvoiceId = allocDto.InvoiceId,
                Amount = allocDto.Amount,
                AppliedDate = payment.VoucherDate,
                InvoiceBalanceBefore = invoice.BalanceAmount,
                InvoiceBalanceAfter = invoice.BalanceAmount - allocDto.Amount,
                Notes = $"Payment allocation for {allocDto.InvoiceNumber}",
                TenantId = TenantId,
                CreatedByUserId = currentUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = currentUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            Context.PaymentAllocations.Add(allocation);

            // Update invoice balance and status
            invoice.PaidAmount += allocDto.Amount;
            invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;

            if (invoice.BalanceAmount <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.BalanceAmount = 0;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
        }

        await Context.SaveChangesAsync();
    }
}
