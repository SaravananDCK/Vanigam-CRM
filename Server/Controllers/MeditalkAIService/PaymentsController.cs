using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Payments)}")]
    public class PaymentsController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    PaymentService service)
    : BaseODataServiceController<Payment, PaymentService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Payment>> BulkSavePaymentWithAllocations([FromBody] PaymentBulkSaveDTO paymentData)
        {
            try
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                Payment payment;
                bool isUpdate = paymentData.Oid.HasValue;

                if (isUpdate)
                {
                    // Update existing payment
                    payment = await context.Payments
                        .Include(p => p.Applications)
                        .FirstOrDefaultAsync(p => p.Oid == paymentData.Oid.Value);

                    if (payment == null)
                        return NotFound("Payment not found");

                    // Update payment properties
                    payment.PartyId = paymentData.PartyId;
                    payment.PaymentAmount = paymentData.PaymentAmount;
                    payment.VoucherDate = paymentData.VoucherDate;
                    payment.PaymentMethod = paymentData.PaymentMethod;
                    payment.ReferenceNumber = paymentData.ReferenceNumber;
                    payment.BankAccountId = paymentData.BankAccountId;
                    payment.Status = paymentData.Status;
                    payment.AllocatedAmount = paymentData.AllocatedAmount;
                    payment.UnallocatedAmount = paymentData.UnallocatedAmount;
                }
                else
                {
                    // Create new payment
                    payment = new Payment
                    {
                        Oid = Guid.NewGuid(),
                        PartyId = paymentData.PartyId,
                        PaymentAmount = paymentData.PaymentAmount,
                        VoucherDate = paymentData.VoucherDate,
                        PaymentMethod = paymentData.PaymentMethod,
                        ReferenceNumber = paymentData.ReferenceNumber,
                        BankAccountId = paymentData.BankAccountId,
                        Status = paymentData.Status,
                        AllocatedAmount = paymentData.AllocatedAmount,
                        UnallocatedAmount = paymentData.UnallocatedAmount,
                        TenantId = service.TenantId,
                        CreatedByUserId = CurrentUser.Id.ToString(),
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = CurrentUser.Id.ToString(),
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    context.Payments.Add(payment);
                }

                await context.SaveChangesAsync();

                // Handle payment allocations
                if (isUpdate)
                {
                    // Remove existing allocations
                    var existingAllocations = await context.PaymentAllocations
                        .Where(a => a.PaymentId == payment.Oid)
                        .ToListAsync();

                    if (existingAllocations.Any())
                    {
                        context.PaymentAllocations.RemoveRange(existingAllocations);
                    }
                }

                // Add new allocations
                foreach (var allocDto in paymentData.Allocations.Where(a => !a.IsDeleted))
                {
                    // Get invoice to update balances
                    var invoice = await context.Invoices.FindAsync(allocDto.InvoiceId);
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
                        TenantId = service.TenantId,
                        CreatedByUserId = CurrentUser.Id.ToString(),
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = CurrentUser.Id.ToString(),
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    context.PaymentAllocations.Add(allocation);

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
                        TenantId = service.TenantId,
                        CreatedByUserId = CurrentUser.Id.ToString(),
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = CurrentUser.Id.ToString(),
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    context.CustomerAdvances.Add(advance);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload payment with applications to return complete object
                await context.Entry(payment).Collection(p => p.Applications).LoadAsync();

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}