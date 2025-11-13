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
    QuoteService service,
    NumberSeriesService numberSeriesService,
    SummaryService<Quote, QuoteStatus> summaryService,
    ILogger<QuotesController> logger)
    : BaseODataServiceController<Quote, QuoteService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("status-summary")]
        [Route("status-summary")]
        public async Task<ActionResult<StatusSummaryResponse<QuoteStatus>>> GetStatusSummary(
            [FromBody] StatusSummaryRequest request)
        {
            try
            {
                logger.LogInformation("Getting Quote status summary with search filter: {SearchFilter}",
                    request.SearchFilter);

                var result = await summaryService.GetStatusSummaryAsync(
                    Context.Quotes,
                    quote => quote.Status,
                    request.SearchFilter,
                    request.AdditionalFilter);

                logger.LogInformation("Quote status summary completed: Total={TotalCount}, Statuses={StatusCount}",
                    result.TotalCount, result.StatusCounts.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting Quote status summary");
                return BadRequest(new { Error = "Failed to retrieve status summary" });
            }
        }

        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Quote>> BulkSaveQuoteWithItems([FromBody] QuoteBulkSaveDTO quoteData)
        {
            try
            {
                // Use service method (quotes don't affect ledger until converted to invoices)
                var savedQuote = await service.BulkSaveQuoteWithItems(quoteData);
                return Ok(savedQuote);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error saving quote: {ex.Message}" });
            }
        }

        [HttpGet("/api/quote/{quoteId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<QuoteItemDTO>>> GetQuoteItemsForEditing(Guid quoteId)
        {
            try
            {
                var items = await context.QuoteItems
                    .Where(qi => qi.VoucherId == quoteId)
                    .Include(qi => qi.Item)
                    .Select(qi => new QuoteItemDTO
                    {
                        Oid = qi.Oid,
                        InventoryItemId = qi.ItemId,
                        Quantity = qi.Quantity,
                        DiscountAmount = qi.DiscountAmount,
                        TaxAmount = qi.TaxAmount,
                        UnitPrice = qi.UnitPrice,
                        TaxCodeId = qi.TaxCodeId,
                        CGSTRate = qi.TaxCode != null ? qi.TaxCode.CGSTRate : 0,
                        SGSTRate = qi.TaxCode != null ? qi.TaxCode.SGSTRate : 0,
                        IGSTRate = qi.TaxCode != null ? qi.TaxCode.IGSTRate : 0,
                        CessRate = qi.TaxCode != null ? qi.TaxCode.CessRate : 0,
                        InventoryItemName = qi.Item != null ? qi.Item.Name : null
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading quote items: {ex.Message}");
            }
        }

        [HttpPost("/api/quote/{quoteId}/convert-to-invoice")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Invoice>> ConvertQuoteToInvoice(Guid quoteId, [FromBody] ConvertQuoteToInvoiceRequest? request = null)
        {
            try
            {
                logger.LogInformation("Converting Quote {QuoteId} to Invoice", quoteId);

                var invoice = await service.ConvertQuoteToInvoiceAsync(quoteId, request?.InvoiceNumber);

                logger.LogInformation("Successfully converted Quote {QuoteId} to Invoice {InvoiceId}", quoteId, invoice.Oid);

                return Ok(invoice);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning("Conversion validation failed for Quote {QuoteId}: {Message}", quoteId, ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error converting Quote {QuoteId} to Invoice", quoteId);
                return BadRequest(new { Error = "Failed to convert quote to invoice" });
            }
        }
    }

    public class ConvertQuoteToInvoiceRequest
    {
        public string? InvoiceNumber { get; set; }
    }
}
