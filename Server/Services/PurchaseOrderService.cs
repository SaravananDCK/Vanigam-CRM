using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseOrderService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
    ILogger<BaseService<PurchaseOrder>> logger)
    : BaseService<PurchaseOrder>(context, logger)
{
    public override DbSet<PurchaseOrder> GetDbSet()
    {
        return Context.PurchaseOrders;
    }
    protected override async Task OnCreatedAsync(PurchaseOrder entity)
    {
        // Generate quote number automatically
        if (string.IsNullOrEmpty(entity.Number))
        {
            entity.Number = await numberSeriesService.GenerateNextNumber("PurchaseOrder", entity.TenantId);
        }

        await base.OnCreatedAsync(entity);
    }

    public async Task<PurchaseOrder> BulkSavePurchaseOrderWithItems(PurchaseOrderBulkSaveDTO purchaseData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            PurchaseOrder purchaseOrder;
            bool isUpdate = purchaseData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing quote
                purchaseOrder = await Context.PurchaseOrders
                    .Include(q => q.VoucherLines)
                    .FirstOrDefaultAsync(q => q.Oid == purchaseData.Oid.Value);

                if (purchaseOrder == null) throw new InvalidOperationException("purchase Order not found");

                // Update quote properties
                purchaseOrder.Status = purchaseData.Status;
                purchaseOrder.PartyId = purchaseData.VendorId;
                purchaseOrder.TotalAmount = purchaseData.TotalAmount;
                purchaseOrder.SubTotal = purchaseData.SubTotal;
                purchaseOrder.TaxAmount = purchaseData.TaxAmount;
                purchaseOrder.DiscountAmount = purchaseData.DiscountAmount;
                purchaseOrder.DiscountPercent = purchaseData.DiscountPercentage;
                purchaseOrder.DiscountType = purchaseData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount;
                purchaseOrder.DueDate = purchaseData.DueDate;
                purchaseOrder.ExpectedDeliveryDate = purchaseData.ExpectedDeliveryDate;
                purchaseOrder.ShippingAddress = purchaseData.ShippingAddress;
                purchaseOrder.ContactPerson = purchaseData.ContactPerson;
                purchaseOrder.Reference = purchaseData.Reference;

                // Handle quote items
                await HandlePurchaseOrderItems(purchaseOrder, purchaseData.Items);

                // Call base update without lifecycle hooks to avoid nested transactions
                Context.PurchaseOrders.Update(purchaseOrder);
                await Context.SaveChangesAsync();
            }
            else
            {
                // Generate quote number
                var quoteNumber = await numberSeriesService.GenerateNextNumber(nameof(PurchaseOrder), TenantId);

                // Create new quote
                purchaseOrder = new PurchaseOrder
                {
                    Oid = Guid.NewGuid(),
                    Number = quoteNumber,
                    Status = purchaseData.Status,
                    PartyId = purchaseData.VendorId,
                    TotalAmount = purchaseData.TotalAmount,
                    SubTotal = purchaseData.SubTotal,
                    TaxAmount = purchaseData.TaxAmount,
                    DiscountAmount = purchaseData.DiscountAmount,
                    DiscountPercent = purchaseData.DiscountPercentage,
                    DiscountType = purchaseData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount,
                    DueDate = purchaseData.DueDate,
                    ExpectedDeliveryDate = purchaseData.ExpectedDeliveryDate,
                    ShippingAddress = purchaseData.ShippingAddress,
                    ContactPerson = purchaseData.ContactPerson,
                    Reference = purchaseData.Reference,
                    TenantId = TenantId
                };

                // Add quote items
                foreach (var itemDto in purchaseData.Items.Where(i => !i.IsDeleted))
                {
                    var newItem = new PurchaseOrderItem
                    {
                        Oid = Guid.NewGuid(),
                        VoucherId = purchaseOrder.Oid,
                        ItemId = itemDto.InventoryItemId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        TaxCodeId = itemDto.TaxCodeId,
                        TaxAmount = itemDto.TaxAmount ?? 0,
                        DiscountAmount = itemDto.DiscountAmount,
                        TenantId = TenantId
                    };
                    Context.PurchaseOrderItems.Add(newItem);
                }

                // Call base create without lifecycle hooks to avoid nested transactions
                Context.PurchaseOrders.Add(purchaseOrder);
                await Context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload quote with items
            var savedQuote = await Context.PurchaseOrders
                .Include(q => q.VoucherLines)
                .ThenInclude(qi => qi.Item)
                .FirstOrDefaultAsync(q => q.Oid == purchaseOrder.Oid);

            return savedQuote!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task HandlePurchaseOrderItems(PurchaseOrder purchaseOrder, List<PurchaseOrderItemDTO> items)
    {
        // Remove deleted items
        var deletedItemIds = items
            .Where(i => i.IsDeleted && i.Oid.HasValue)
            .Select(i => i.Oid.Value)
            .ToList();

        if (deletedItemIds.Any())
        {
            var itemsToDelete = purchaseOrder.VoucherLines.OfType<PurchaseOrderItem>().Where(i => deletedItemIds.Contains(i.Oid)).ToList();
            Context.PurchaseOrderItems.RemoveRange(itemsToDelete);
        }

        // Add or update items
        foreach (var itemDto in items.Where(i => !i.IsDeleted))
        {
            if (itemDto.IsNew)
            {
                // Add new item
                var newItem = new PurchaseOrderItem
                {
                    Oid = Guid.NewGuid(),
                    VoucherId = purchaseOrder.Oid,
                    ItemId = itemDto.InventoryItemId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TaxCodeId = itemDto.TaxCodeId,
                    TaxAmount = itemDto.TaxAmount ?? 0,
                    DiscountAmount = itemDto.DiscountAmount,
                    TenantId = TenantId
                };

                Context.PurchaseOrderItems.Add(newItem);
            }
            else if (itemDto.Oid.HasValue)
            {
                // Update existing item
                var existingItem = await Context.PurchaseOrderItems
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
}
