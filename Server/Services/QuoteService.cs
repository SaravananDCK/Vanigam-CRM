using Microsoft.EntityFrameworkCore;
using NodaTime;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class QuoteService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
    ILogger<BaseService<Quote>> logger)
    : BaseService<Quote>(context, logger)
{
    public override DbSet<Quote> GetDbSet()
    {
        return Context.Quotes;
    }
    protected override async Task OnCreatedAsync(Quote entity)
    {
        // Generate quote number automatically
        if (string.IsNullOrEmpty(entity.Number))
        {
            entity.Number = await numberSeriesService.GenerateNextNumber("Quote", entity.TenantId);
        }

        await base.OnCreatedAsync(entity);
    }

    /// <summary>
    /// Bulk save quote with items. Handles create/update of quote and its items.
    /// Note: Quotes do not affect ledger entries until they are converted to invoices.
    /// </summary>
    public async Task<Quote> BulkSaveQuoteWithItems(QuoteBulkSaveDTO quoteData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Quote quote;
            bool isUpdate = quoteData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing quote
                quote = await Context.Quotes
                    .Include(q => q.VoucherLines)
                    .FirstOrDefaultAsync(q => q.Oid == quoteData.Oid.Value);

                if (quote == null)
                    throw new InvalidOperationException("Quote not found");

                // Update quote properties
                quote.Status = quoteData.Status;
                quote.OpportunityId = quoteData.OpportunityId;
                quote.PartyId = quoteData.CustomerId;
                quote.JobId = quoteData.JobId;
                quote.TotalAmount = quoteData.TotalAmount;
                quote.SubTotal = quoteData.SubTotal;
                quote.TaxAmount = quoteData.TaxAmount;
                quote.DiscountAmount = quoteData.DiscountAmount;
                quote.DiscountPercent = quoteData.DiscountPercentage;
                quote.DiscountType = quoteData.DiscountType;
                quote.CGSTAmount = quoteData.CGSTAmount;
                quote.SGSTAmount = quoteData.SGSTAmount;
                quote.IGSTAmount = quoteData.IGSTAmount;
                quote.CessAmount = quoteData.CessAmount;

                // Handle quote items
                await HandleQuoteItems(quote, quoteData.Items);

                // Call base update without lifecycle hooks to avoid nested transactions
                Context.Quotes.Update(quote);
                await Context.SaveChangesAsync();
            }
            else
            {
                // Generate quote number
                var quoteNumber = await numberSeriesService.GenerateNextNumber(nameof(Quote), TenantId);

                // Create new quote
                quote = new Quote
                {
                    Oid = Guid.NewGuid(),
                    Number = quoteNumber,
                    Status = quoteData.Status,
                    OpportunityId = quoteData.OpportunityId,
                    PartyId = quoteData.CustomerId,
                    JobId = quoteData.JobId,
                    TotalAmount = quoteData.TotalAmount,
                    SubTotal = quoteData.SubTotal,
                    TaxAmount = quoteData.TaxAmount,
                    DiscountAmount = quoteData.DiscountAmount,
                    DiscountPercent = quoteData.DiscountPercentage,
                    DiscountType = quoteData.DiscountType,
                    TenantId = TenantId,
                    CGSTAmount = quoteData.CGSTAmount,
                    SGSTAmount = quoteData.SGSTAmount,
                    IGSTAmount = quoteData.IGSTAmount,
                    CessAmount = quoteData.CessAmount,
                };

                // Add quote items
                foreach (var itemDto in quoteData.Items.Where(i => !i.IsDeleted))
                {
                    if (itemDto.InventoryItemId != null)
                    {
                        var newItem = new QuoteItem
                        {
                            Oid = Guid.NewGuid(),
                            VoucherId = quote.Oid,
                            ItemId = itemDto.InventoryItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            TaxCodeId = itemDto.TaxCodeId,
                            TaxAmount = itemDto.TaxAmount ?? 0,
                            DiscountAmount = itemDto.DiscountAmount,
                            TenantId = TenantId
                        };
                        Context.QuoteItems.Add(newItem);
                    }
                }
                // Call base create without lifecycle hooks to avoid nested transactions
                Context.Quotes.Add(quote);
                await Context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload quote with items
            var savedQuote = await Context.Quotes
                .Include(q => q.VoucherLines)
                .ThenInclude(qi => qi.Item)
                .FirstOrDefaultAsync(q => q.Oid == quote.Oid);

            return savedQuote!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles adding, updating, and deleting quote items
    /// </summary>
    private async Task HandleQuoteItems(Quote quote, List<QuoteItemDTO> items)
    {
        // Remove deleted items
        var deletedItemIds = items
            .Where(i => i.IsDeleted && i.Oid.HasValue)
            .Select(i => i.Oid.Value)
            .ToList();

        if (deletedItemIds.Any())
        {
            var itemsToDelete = quote.VoucherLines.OfType<QuoteItem>().Where(i => deletedItemIds.Contains(i.Oid)).ToList();
            Context.QuoteItems.RemoveRange(itemsToDelete);
        }

        // Add or update items
        foreach (var itemDto in items.Where(i => !i.IsDeleted))
        {
            if (itemDto.IsNew && itemDto.InventoryItemId != null)
            {
                // Add new item
                var newItem = new QuoteItem
                {
                    Oid = Guid.NewGuid(),
                    VoucherId = quote.Oid,
                    ItemId = itemDto.InventoryItemId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TaxCodeId = itemDto.TaxCodeId,
                    TaxAmount = itemDto.TaxAmount ?? 0,
                    DiscountAmount = itemDto.DiscountAmount,
                    TenantId = TenantId
                };

                Context.QuoteItems.Add(newItem);
            }
            else if (itemDto.Oid.HasValue)
            {
                // Update existing item
                var existingItem = await Context.QuoteItems
                    .FirstOrDefaultAsync(qi => qi.Oid == itemDto.Oid.Value);

                if (existingItem != null)
                {
                    existingItem.ItemId = itemDto.InventoryItemId;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.UnitPrice = itemDto.UnitPrice;
                    existingItem.TaxCodeId = itemDto.TaxCodeId;
                    existingItem.TaxAmount = itemDto.TaxAmount ?? 0;
                    existingItem.DiscountAmount = itemDto.DiscountAmount;
                }
            }
        }

        await Context.SaveChangesAsync();
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
                .Include(q => q.VoucherLines)
                .ThenInclude(vl => vl.Item)
                .Include(q => q.Party)
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
                QuoteId = quote.Oid,
                Number = invoiceNumber,
                Status = InvoiceStatus.Draft,
                PartyId = quote.PartyId,
                TotalAmount = quote.TotalAmount,
                TaxAmount = quote.TaxAmount,
                SubTotal = quote.SubTotal,
                DiscountAmount = quote.DiscountAmount,
                DiscountPercent = quote.DiscountPercent,
                DiscountType = quote.DiscountType,
                CreatedByUserId = quote.UpdatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = quote.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            // Add invoice to context
            Context.Invoices.Add(invoice);
            quote.Status = QuoteStatus.Converted;
            Context.Quotes.Update(quote);
            // Create invoice items from quote items (use VoucherLines with TPH)
            foreach (var quoteItem in quote.VoucherLines.OfType<QuoteItem>())
            {
                var invoiceItem = new InvoiceItem
                {
                    Oid = Guid.NewGuid(),
                    TenantId = quote.TenantId,
                    VoucherId = invoice.Oid,
                    ItemId = quoteItem.ItemId,
                    Quantity = quoteItem.Quantity,
                    UnitPrice = quoteItem.UnitPrice,
                    TaxCodeId = quoteItem.TaxCodeId,
                    TaxAmount = quoteItem.TaxAmount,
                    DiscountAmount = quoteItem.DiscountAmount,
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
