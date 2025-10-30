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
        public async Task<ActionResult<PurchaseInvoice>> BulkSavePurchaseInvoiceWithItems([FromBody] PurchaseInvoiceBulkSaveDTO purchaseInvoiceData)
        {
            try
            {
                var savedPurchaseInvoice = await service.BulkSavePurchaseInvoiceWithItems(purchaseInvoiceData);
                return Ok(savedPurchaseInvoice);
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

        [HttpGet("/api/purchaseinvoice/{purchaseInvoiceId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<PurchaseInvoiceItemDTO>>> GetPurchaseInvoiceItemsForEditing(Guid purchaseInvoiceId)
        {
            try
            {
                var items = await context.PurchaseInvoiceItems
                    .Where(i => i.VoucherId == purchaseInvoiceId)
                    .Include(i => i.Item)
                    .Include(i => i.TaxCode)
                    .Select(i => new PurchaseInvoiceItemDTO
                    {
                        Oid = i.Oid,
                        InventoryItemId = i.ItemId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        TaxCodeId = i.TaxCodeId,
                        CGSTRate = i.TaxCode != null ? i.TaxCode.CGSTRate : 0,
                        SGSTRate = i.TaxCode != null ? i.TaxCode.SGSTRate : 0,
                        IGSTRate = i.TaxCode != null ? i.TaxCode.IGSTRate : 0,
                        CessRate = i.TaxCode != null ? i.TaxCode.CessRate : 0,
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
