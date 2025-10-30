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
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.PurchaseInvoices)}")]
    public class PurchaseInvoicesController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    PurchaseInvoiceService service)
    : BaseODataServiceController<PurchaseInvoice, PurchaseInvoiceService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<PurchaseInvoice>> BulkSaveInvoiceWithPayments([FromBody] PurchaseInvoiceBulkSaveDTO invoiceData)
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
                return BadRequest(new { error = $"Error saving purchase invoice: {ex.Message}" });
            }
        }

        [HttpGet("/api/purchaseInvoice/{invoiceId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<PurchaseInvoiceItemDTO>>> GetInvoiceItemsForEditing(Guid invoiceId)
        {
            try
            {
                var items = await context.PurchaseInvoiceItems
                    .Where(i => i.VoucherId == invoiceId)
                    .Include(i => i.Item)
                    .Select(i => new PurchaseInvoiceItemDTO
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
                return BadRequest($"Error loading purchase invoice items: {ex.Message}");
            }
        }
    }
}
