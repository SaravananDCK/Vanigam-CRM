using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers.MeditalkAIService
{
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.Invoices)}")]
    public class InvoicesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    InvoiceService service)
    : BaseODataServiceController<Invoice, InvoiceService>(context, userManager, roleManager,
        service, null)
    {

        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Invoice>> BulkSaveInvoiceWithPayments([FromBody] InvoiceBulkSaveDTO invoiceData)
        {
            try
            {
                // Use service method which ensures proper ledger posting
                var savedInvoice = await service.BulkSaveInvoiceWithItems(invoiceData);
                return Ok(savedInvoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error saving invoice: {ex.Message}" });
            }
        }

        [HttpGet("/api/invoice/{invoiceId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<InvoiceItemDTO>>> GetInvoiceItemsForEditing(Guid invoiceId)
        {
            try
            {
                var items = await context.InvoiceItems
                    .Where(i => i.VoucherId == invoiceId)
                    .Include(i => i.Item)
                    .Select(i => new InvoiceItemDTO
                    {
                        Oid = i.Oid,
                        InventoryItemId = i.ItemId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        InventoryItemName = i.Item != null ? i.Item.Name : null
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading invoice items: {ex.Message}");
            }
        }
    }
}
