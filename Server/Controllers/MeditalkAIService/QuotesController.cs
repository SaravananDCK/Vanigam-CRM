using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Quotes)}")]
    public class QuotesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    QuoteService service)
    : BaseODataServiceController<Quote, QuoteService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("/api/quote/bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Quote>> BulkSaveQuoteWithItems([FromBody] QuoteBulkSaveDTO quoteData)
        {
            try
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                Quote quote;
                bool isUpdate = quoteData.Oid.HasValue;

                if (isUpdate)
                {
                    // Update existing quote
                    quote = await context.Quotes
                        .Include(q => q.Items)
                        .FirstOrDefaultAsync(q => q.Oid == quoteData.Oid.Value);

                    if (quote == null)
                        return NotFound("Quote not found");

                    // Update quote properties
                    quote.Title = quoteData.Title;
                    quote.Status = quoteData.Status;
                    quote.OpportunityId = quoteData.OpportunityId;
                    quote.CustomerId = quoteData.CustomerId;
                    quote.JobId = quoteData.JobId;
                    quote.TotalAmount = quoteData.TotalAmount;
                }
                else
                {
                    // Create new quote
                    quote = new Quote
                    {
                        Oid = Guid.NewGuid(),
                        Title = quoteData.Title,
                        Status = quoteData.Status,
                        OpportunityId = quoteData.OpportunityId,
                        CustomerId = quoteData.CustomerId,
                        JobId = quoteData.JobId,
                        TotalAmount = quoteData.TotalAmount,
                        TenantId = CurrentUser.TenantId
                    };

                    context.Quotes.Add(quote);
                }

                await context.SaveChangesAsync();

                // Handle quote items
                if (isUpdate)
                {
                    // Remove deleted items
                    var deletedItemIds = quoteData.Items
                        .Where(i => i.IsDeleted && i.Oid.HasValue)
                        .Select(i => i.Oid.Value)
                        .ToList();

                    if (deletedItemIds.Any())
                    {
                        var itemsToDelete = quote.Items.Where(i => deletedItemIds.Contains(i.Oid)).ToList();
                        context.QuoteItems.RemoveRange(itemsToDelete);
                    }
                }

                // Add or update items
                foreach (var itemDto in quoteData.Items.Where(i => !i.IsDeleted))
                {
                    if (itemDto.IsNew)
                    {
                        // Add new item
                        var newItem = new QuoteItem
                        {
                            Oid = Guid.NewGuid(),
                            QuoteId = quote.Oid,
                            InventoryItemId = itemDto.InventoryItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            TenantId = CurrentUser.TenantId
                        };

                        context.QuoteItems.Add(newItem);
                    }
                    else if (itemDto.Oid.HasValue)
                    {
                        // Update existing item
                        var existingItem = await context.QuoteItems
                            .FirstOrDefaultAsync(qi => qi.Oid == itemDto.Oid.Value);

                        if (existingItem != null)
                        {
                            existingItem.InventoryItemId = itemDto.InventoryItemId;
                            existingItem.Quantity = itemDto.Quantity;
                            existingItem.UnitPrice = itemDto.UnitPrice;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated quote with items
                var savedQuote = await context.Quotes
                    .Include(q => q.Items)
                    .ThenInclude(qi => qi.InventoryItem)
                    .FirstOrDefaultAsync(q => q.Oid == quote.Oid);

                return Ok(savedQuote);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error saving quote: {ex.Message}");
            }
        }

        [HttpGet("/api/quote/{quoteId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<QuoteItemDTO>>> GetQuoteItemsForEditing(Guid quoteId)
        {
            try
            {
                var items = await context.QuoteItems
                    .Where(qi => qi.QuoteId == quoteId)
                    .Include(qi => qi.InventoryItem)
                    .Select(qi => new QuoteItemDTO
                    {
                        Oid = qi.Oid,
                        InventoryItemId = qi.InventoryItemId,
                        Quantity = qi.Quantity,
                        UnitPrice = qi.UnitPrice,
                        InventoryItemName = qi.InventoryItem != null ? qi.InventoryItem.Name : null
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading quote items: {ex.Message}");
            }
        }
    }
}