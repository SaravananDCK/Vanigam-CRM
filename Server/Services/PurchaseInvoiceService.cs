using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseInvoiceService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
    ILogger<BaseService<PurchaseInvoice>> logger,
    LedgerPostingService ledgerPostingService)
    : BaseService<PurchaseInvoice>(context, logger)
{
    public override DbSet<PurchaseInvoice> GetDbSet()
    {
        return Context.PurchaseInvoices;
    }

    /// <summary>
    /// Creates a new purchase invoice and posts it to the ledger if status is Posted.
    /// </summary>
    protected override async Task OnCreatedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await base.OnCreatedAsync(purchaseInvoice);

            // Post to ledger if purchase invoice is already in Posted status
            if (purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
            {
                await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                await Context.SaveChangesAsync();

                // Validate entries balance
                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                }
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates a purchase invoice. If status changes to Posted, creates ledger entries.
    /// If status changes from Posted to another status, reverses ledger entries.
    /// </summary>
    protected override async Task OnUpdatedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the original purchase invoice to check status changes
            var original = await Context.PurchaseInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Oid == purchaseInvoice.Oid);

            await base.OnUpdatedAsync(purchaseInvoice);

            // Handle status changes
            if (original != null)
            {
                // Status changed from non-Posted to Posted
                if (original.Status != PurchaseInvoiceStatus.Posted && purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                    }
                }
                // Status changed from Posted to non-Posted (reversal)
                else if (original.Status == PurchaseInvoiceStatus.Posted && purchaseInvoice.Status != PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.ReverseVoucherEntries(purchaseInvoice.Oid, $"Purchase Invoice {purchaseInvoice.Number} status changed to {purchaseInvoice.Status}");
                    await Context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Deletes a purchase invoice. If it was posted, reverses the ledger entries first.
    /// </summary>
    protected override async Task<bool> OnDeletedAsync(PurchaseInvoice purchaseInvoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            if (purchaseInvoice == null)
                return false;

            // Reverse ledger entries if purchase invoice was posted
            if (purchaseInvoice.Status >= PurchaseInvoiceStatus.Posted)
            {
                await ledgerPostingService.ReverseVoucherEntries(purchaseInvoice.Oid, $"Purchase Invoice {purchaseInvoice.Number} deleted");
                await Context.SaveChangesAsync();
            }

            var deleted = await base.DeleteAsync(purchaseInvoice);
            await transaction.CommitAsync();
            return deleted;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Bulk save purchase invoice with items. Handles create/update of purchase invoice and its items,
    /// and automatically posts to ledger if status is Posted.
    /// </summary>
    public async Task<PurchaseInvoice> BulkSavePurchaseInvoiceWithItems(PurchaseInvoiceBulkSaveDTO purchaseInvoiceData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            PurchaseInvoice purchaseInvoice;
            bool isUpdate = purchaseInvoiceData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing purchase invoice
                purchaseInvoice = await Context.PurchaseInvoices
                    .Include(pi => pi.VoucherLines)
                    .FirstOrDefaultAsync(pi => pi.Oid == purchaseInvoiceData.Oid.Value);

                if (purchaseInvoice == null)
                    throw new InvalidOperationException("Purchase Invoice not found");

                // Update purchase invoice properties
                purchaseInvoice.Number = purchaseInvoiceData.Number;
                purchaseInvoice.Status = purchaseInvoiceData.Status;
                purchaseInvoice.PartyId = purchaseInvoiceData.PartyId;
                purchaseInvoice.VendorInvoiceNumber = purchaseInvoiceData.VendorInvoiceNumber;
                purchaseInvoice.ReceivedDate = purchaseInvoiceData.ReceivedDate;
                purchaseInvoice.TotalAmount = purchaseInvoiceData.TotalAmount;
                purchaseInvoice.SubTotal = purchaseInvoiceData.SubTotal;
                purchaseInvoice.TaxAmount = purchaseInvoiceData.TaxAmount;
                purchaseInvoice.CGSTAmount = purchaseInvoiceData.CGSTAmount;
                purchaseInvoice.SGSTAmount = purchaseInvoiceData.SGSTAmount;
                purchaseInvoice.IGSTAmount = purchaseInvoiceData.IGSTAmount;
                purchaseInvoice.CessAmount = purchaseInvoiceData.CessAmount;
                purchaseInvoice.VoucherDate = purchaseInvoiceData.VoucherDate;
                purchaseInvoice.DueDate = purchaseInvoiceData.DueDate;
                purchaseInvoice.DiscountAmount = purchaseInvoiceData.DiscountAmount;
                purchaseInvoice.DiscountPercent = purchaseInvoiceData.DiscountPercentage;
                purchaseInvoice.DiscountType = purchaseInvoiceData.DiscountType;
                purchaseInvoice.PurchaseOrderId = purchaseInvoiceData.PurchaseOrderId;

                // Handle purchase invoice items
                await HandlePurchaseInvoiceItems(purchaseInvoice, purchaseInvoiceData.Items);

                Context.PurchaseInvoices.Update(purchaseInvoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting if status is Posted
                if (purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                    }
                }
            }
            else
            {
                var invoiceNumber = await numberSeriesService.GenerateNextNumber(nameof(PurchaseInvoice), TenantId);
                // Create new purchase invoice
                purchaseInvoice = new PurchaseInvoice
                {
                    Oid = Guid.NewGuid(),
                    Number = invoiceNumber,
                    Status = purchaseInvoiceData.Status,
                    PartyId = purchaseInvoiceData.PartyId,
                    VendorInvoiceNumber = purchaseInvoiceData.VendorInvoiceNumber,
                    ReceivedDate = purchaseInvoiceData.ReceivedDate,
                    TotalAmount = purchaseInvoiceData.TotalAmount,
                    SubTotal = purchaseInvoiceData.SubTotal,
                    TaxAmount = purchaseInvoiceData.TaxAmount,
                    CGSTAmount = purchaseInvoiceData.CGSTAmount,
                    SGSTAmount = purchaseInvoiceData.SGSTAmount,
                    IGSTAmount = purchaseInvoiceData.IGSTAmount,
                    CessAmount = purchaseInvoiceData.CessAmount,
                    VoucherDate = purchaseInvoiceData.VoucherDate,
                    DueDate = purchaseInvoiceData.DueDate,
                    DiscountAmount = purchaseInvoiceData.DiscountAmount,
                    DiscountPercent = purchaseInvoiceData.DiscountPercentage,
                    DiscountType = purchaseInvoiceData.DiscountType,
                    PurchaseOrderId = purchaseInvoiceData.PurchaseOrderId,
                    TenantId = TenantId
                };

                // Add purchase invoice items
                foreach (var itemDto in purchaseInvoiceData.Items.Where(i => !i.IsDeleted))
                {
                    if (itemDto.InventoryItemId != null)
                    {
                        var newItem = new PurchaseInvoiceItem
                        {
                            Oid = Guid.NewGuid(),
                            VoucherId = purchaseInvoice.Oid,
                            ItemId = itemDto.InventoryItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            DiscountAmount = itemDto.DiscountAmount,
                            TaxAmount = itemDto.TaxAmount ?? 0,
                            TaxCodeId = itemDto.TaxCodeId,
                            TenantId = TenantId
                        };
                        Context.PurchaseInvoiceItems.Add(newItem);
                    }
                }

                Context.PurchaseInvoices.Add(purchaseInvoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting if status is Posted
                if (purchaseInvoice.Status == PurchaseInvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostPurchaseInvoiceToLedger(purchaseInvoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(purchaseInvoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Purchase Invoice {purchaseInvoice.Number} are not balanced");
                    }
                }
            }

            await transaction.CommitAsync();

            // Reload purchase invoice with items
            var savedPurchaseInvoice = await Context.PurchaseInvoices
                .Include(pi => pi.VoucherLines)
                .FirstOrDefaultAsync(pi => pi.Oid == purchaseInvoice.Oid);

            return savedPurchaseInvoice!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles adding, updating, and deleting purchase invoice items
    /// </summary>
    private async Task HandlePurchaseInvoiceItems(PurchaseInvoice purchaseInvoice, List<PurchaseInvoiceItemDTO> items)
    {
        // Remove deleted items
        var deletedItemIds = items
            .Where(i => i.IsDeleted && i.Oid.HasValue)
            .Select(i => i.Oid.Value)
            .ToList();

        if (deletedItemIds.Any())
        {
            var itemsToDelete = purchaseInvoice.VoucherLines.OfType<PurchaseInvoiceItem>().Where(i => deletedItemIds.Contains(i.Oid)).ToList();
            Context.PurchaseInvoiceItems.RemoveRange(itemsToDelete);
        }

        // Add or update items
        foreach (var itemDto in items.Where(i => !i.IsDeleted))
        {
            if (itemDto.IsNew && itemDto.InventoryItemId != null)
            {
                // Add new item
                var newItem = new PurchaseInvoiceItem
                {
                    Oid = Guid.NewGuid(),
                    VoucherId = purchaseInvoice.Oid,
                    ItemId = itemDto.InventoryItemId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    TaxAmount = itemDto.TaxAmount ?? 0,
                    TaxCodeId = itemDto.TaxCodeId,
                    TenantId = TenantId
                };

                Context.PurchaseInvoiceItems.Add(newItem);
            }
            else if (itemDto.Oid.HasValue)
            {
                // Update existing item
                var existingItem = await Context.PurchaseInvoiceItems
                    .FirstOrDefaultAsync(i => i.Oid == itemDto.Oid.Value);

                if (existingItem != null)
                {
                    existingItem.ItemId = itemDto.InventoryItemId;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.UnitPrice = itemDto.UnitPrice;
                    existingItem.DiscountAmount = itemDto.DiscountAmount;
                    existingItem.TaxAmount = itemDto.TaxAmount ?? 0;
                    existingItem.TaxCodeId = itemDto.TaxCodeId;
                }
            }
        }
    }
}
