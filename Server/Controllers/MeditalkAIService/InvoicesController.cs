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
                using var transaction = await context.Database.BeginTransactionAsync();

                Invoice invoice;
                bool isUpdate = invoiceData.Oid.HasValue;

                if (isUpdate)
                {
                    // Update existing invoice
                    invoice = await context.Invoices
                        .Include(i => i.Items)
                        .FirstOrDefaultAsync(i => i.Oid == invoiceData.Oid.Value);

                    if (invoice == null)
                        return NotFound("Invoice not found");

                    // Update invoice properties
                    invoice.Number = invoiceData.Number;
                    invoice.Status = invoiceData.Status;
                    invoice.PartyId = invoiceData.PartyId;
                    invoice.TotalAmount = invoiceData.TotalAmount;
                }
                else
                {
                    // Create new invoice
                    invoice = new Invoice
                    {
                        Oid = Guid.NewGuid(),
                        Number = invoiceData.Number,
                        Status = invoiceData.Status,
                        PartyId = invoiceData.PartyId,
                        TotalAmount = invoiceData.TotalAmount,
                        TenantId = service.TenantId
                    };

                    context.Invoices.Add(invoice);
                }

                await context.SaveChangesAsync();

                // Handle invoice items
                if (isUpdate)
                {
                    // Remove deleted items
                    var deletedItemIds = invoiceData.Items
                        .Where(i => i.IsDeleted && i.Oid.HasValue)
                        .Select(i => i.Oid.Value)
                        .ToList();

                    if (deletedItemIds.Any())
                    {
                        var itemsToDelete = invoice.Items.Where(i => deletedItemIds.Contains(i.Oid)).ToList();
                        context.InvoiceItems.RemoveRange(itemsToDelete);
                    }
                }

                // Add or update items
                foreach (var itemDto in invoiceData.Items.Where(i => !i.IsDeleted))
                {
                    if (itemDto.IsNew)
                    {
                        // Add new item
                        var newItem = new InvoiceItem
                        {
                            Oid = Guid.NewGuid(),
                            VoucherId = invoice.Oid,
                            ItemId = itemDto.InventoryItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            TenantId = service.TenantId
                        };

                        context.InvoiceItems.Add(newItem);
                    }
                    else if (itemDto.Oid.HasValue)
                    {
                        // Update existing item
                        var existingItem = await context.InvoiceItems
                            .FirstOrDefaultAsync(i => i.Oid == itemDto.Oid.Value);

                        if (existingItem != null)
                        {
                            existingItem.ItemId = itemDto.InventoryItemId;
                            existingItem.Quantity = itemDto.Quantity;
                            existingItem.UnitPrice = itemDto.UnitPrice;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated invoice with items
                var savedInvoice = await context.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Oid == invoice.Oid);
                return Ok(savedInvoice);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error saving invoice: {ex.Message}");
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