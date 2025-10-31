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
    [Route($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.PurchaseOrders)}")]
    public class PurchaseOrdersController(
    VanigamAccountingDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    PurchaseOrderService service)
    : BaseODataServiceController<PurchaseOrder, PurchaseOrderService>(context, userManager, roleManager,
        service, null)
    {
        [HttpPost("bulk-save")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<PurchaseOrder>> BulkSavePurchaseWithItems([FromBody] PurchaseOrderBulkSaveDTO purchaseData)
        {
            try
            {
                // Use service method (quotes don't affect ledger until converted to invoices)
                var savedQuote = await service.BulkSavePurchaseOrderWithItems(purchaseData);
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

        [HttpGet("/api/purchaseOrder/{purchaseOrderId}/items-for-editing")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<PurchaseOrderItemDTO>>> GetPurchaseOrderItemsForEditing(Guid purchaseOrderId)
        {
            try
            {
                var items = await context.PurchaseOrderItems
                    .Where(qi => qi.VoucherId == purchaseOrderId)
                    .Include(qi => qi.Item)
                    .Select(qi => new PurchaseOrderItemDTO
                    {
                        Oid = qi.Oid,
                        InventoryItemId = qi.ItemId,
                        Quantity = qi.Quantity,
                        DiscountAmount = qi.DiscountAmount,
                        TaxAmount = qi.TaxAmount,
                        UnitPrice = qi.UnitPrice,
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
    }
}
