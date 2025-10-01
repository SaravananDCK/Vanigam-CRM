using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class QuoteService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Quote>> logger)
    : BaseService<Quote>(context, logger)
{
    public override DbSet<Quote> GetDbSet()
    {
        return Context.Quotes;
    }

    /// <summary>
    /// Converts a Quote to an Invoice
    /// </summary>
    /// <param name="quoteId">ID of the Quote to convert</param>
    /// <param name="invoiceNumber">Number for the new Invoice (optional, will auto-generate if not provided)</param>
    /// <returns>The created Invoice</returns>
    public async Task<Invoice> ConvertQuoteToInvoiceAsync(Guid quoteId, string? invoiceNumber = null)
    {
        Logger.LogInformation("Converting Quote {QuoteId} to Invoice", quoteId);

        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the quote with its items
            var quote = await Context.Quotes
                .Include(q => q.Items)
                .ThenInclude(qi => qi.InventoryItem)
                .Include(q => q.Customer)
                .Include(q => q.Job)
                .FirstOrDefaultAsync(q => q.Oid == quoteId);

            if (quote == null)
            {
                throw new InvalidOperationException($"Quote with ID {quoteId} not found");
            }

            if (quote.Status != QuoteStatus.Accepted)
            {
                throw new InvalidOperationException("Only accepted quotes can be converted to invoices");
            }

            // Check if invoice already exists for this quote
            var existingInvoice = await Context.Invoices
                .FirstOrDefaultAsync(i => i.Number.Contains(quote.Number));

            if (existingInvoice != null)
            {
                throw new InvalidOperationException("Invoice already exists for this quote");
            }

            // Generate invoice number if not provided
            if (string.IsNullOrEmpty(invoiceNumber))
            {
                var invoiceCount = await Context.Invoices.CountAsync() + 1;
                invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{invoiceCount:D4}";
            }

            // Create invoice from quote
            var invoice = new Invoice
            {
                Oid = Guid.NewGuid(),
                TenantId = quote.TenantId,
                Number = invoiceNumber,
                Status = InvoiceStatus.Draft,
                CustomerId = quote.CustomerId,
                JobId = quote.JobId,
                TotalAmount = quote.TotalAmount,
                CreatedByUserId = quote.UpdatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = quote.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            // Add invoice to context
            Context.Invoices.Add(invoice);

            // Create invoice items from quote items
            foreach (var quoteItem in quote.Items)
            {
                var invoiceItem = new InvoiceItem
                {
                    Oid = Guid.NewGuid(),
                    TenantId = quote.TenantId,
                    InvoiceId = invoice.Oid,
                    InventoryItemId = quoteItem.InventoryItemId,
                    Quantity = quoteItem.Quantity,
                    UnitPrice = quoteItem.UnitPrice,
                    CreatedByUserId = quote.UpdatedByUserId,
                    CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    UpdatedByUserId = quote.UpdatedByUserId,
                    UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    IsNotDeleted = true
                };

                Context.InvoiceItems.Add(invoiceItem);
            }

            // Update quote status (optional - mark as converted)
            // Note: QuoteStatus doesn't have a "Converted" status, so we'll leave it as Accepted

            // Create an activity to track the conversion
            var activity = new Activity
            {
                Oid = Guid.NewGuid(),
                TenantId = quote.TenantId,
                //JobId = quote.JobId,
                Type = ActivityType.CustomerConversion,
                Subject = $"Quote converted to Invoice: {invoiceNumber}",
                Description = $"Quote '{quote.Number}' was successfully converted to invoice '{invoiceNumber}' with total amount {invoice.TotalAmount:C}",
                ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                Status = ActivityStatus.Completed,
                CreatedByUserId = quote.UpdatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = quote.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            Context.Activities.Add(activity);

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            Logger.LogInformation("Successfully converted Quote {QuoteId} to Invoice {InvoiceId}", quoteId, invoice.Oid);
            return invoice;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Logger.LogError(ex, "Error converting Quote {QuoteId} to Invoice", quoteId);
            throw;
        }
    }
}