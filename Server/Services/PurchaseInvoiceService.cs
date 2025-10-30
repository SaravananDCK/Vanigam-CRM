using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class PurchaseInvoiceService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
    ILogger<BaseService<PurchaseInvoice>> logger,
    LedgerPostingService ledgerPostingService,
    ContractAutoCreationService contractAutoCreationService)
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

    public async Task<PurchaseInvoice> BulkSaveInvoiceWithItems(PurchaseInvoiceBulkSaveDTO invoiceData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            PurchaseInvoice PurchaseInvoice;
            bool isUpdate = invoiceData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing invoice
                PurchaseInvoice = await Context.PurchaseInvoices
                    .Include(i => i.VoucherLines)
                    .FirstOrDefaultAsync(i => i.Oid == invoiceData.Oid.Value);

                if (PurchaseInvoice == null)
                    throw new InvalidOperationException("Invoice not found");

                // Update invoice properties
                PurchaseInvoice.Number = invoiceData.Number;
                PurchaseInvoice.Status = invoiceData.Status;
                PurchaseInvoice.PartyId = invoiceData.PartyId;
                PurchaseInvoice.TotalAmount = invoiceData.TotalAmount;
                PurchaseInvoice.SubTotal = invoiceData.SubTotal;
                PurchaseInvoice.TaxAmount = invoiceData.TaxAmount;
                PurchaseInvoice.VoucherDate = invoiceData.VoucherDate;
                PurchaseInvoice.DueDate = invoiceData.DueDate;
                PurchaseInvoice.ReceivedDate = invoiceData.ReceivedDate;
                PurchaseInvoice.DiscountAmount = invoiceData.DiscountAmount;
                PurchaseInvoice.DiscountPercent = invoiceData.DiscountPercentage;
                PurchaseInvoice.DiscountType = invoiceData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount;
                // Handle invoice items
                await HandleInvoiceItems(PurchaseInvoice, invoiceData.Items);

                // Call base update without lifecycle hooks to avoid nested transactions
                Context.PurchaseInvoices.Update(PurchaseInvoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting and contract creation
                //if (PurchaseInvoice.Status >= PurchaseInvoiceStatus.Posted)
                //{
                //    await ledgerPostingService.PostInvoiceToLedger(PurchaseInvoice);
                //    await Context.SaveChangesAsync();

                //    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(PurchaseInvoice.Oid);
                //    if (!isBalanced)
                //    {
                //        throw new InvalidOperationException($"Ledger entries for Invoice {PurchaseInvoice.Number} are not balanced");
                //    }

                //    // Auto-create Warranty/Guarantee contracts
                //    await contractAutoCreationService.ProcessInvoiceForContracts(PurchaseInvoice);
                //    await Context.SaveChangesAsync();
                //}
            }
            else
            {
                var invoiceNumber = await numberSeriesService.GenerateNextNumber(nameof(PurchaseInvoice), TenantId);
                // Create new invoice
                PurchaseInvoice = new PurchaseInvoice
                {
                    Oid = Guid.NewGuid(),
                    Number = invoiceNumber,
                    Status = invoiceData.Status,
                    PartyId = invoiceData.PartyId,
                    TotalAmount = invoiceData.TotalAmount,
                    SubTotal = invoiceData.SubTotal,
                    TaxAmount = invoiceData.TaxAmount,
                    VoucherDate = invoiceData.VoucherDate,
                    DueDate = invoiceData.DueDate,
                    ReceivedDate = invoiceData.ReceivedDate,
                    DiscountAmount = invoiceData.DiscountAmount,
                    DiscountPercent = invoiceData.DiscountPercentage,
                    DiscountType = invoiceData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount,
                    TenantId = TenantId
                };

                // Add invoice items
                foreach (var itemDto in invoiceData.Items.Where(i => !i.IsDeleted))
                {
                    var newItem = new PurchaseInvoiceItem
                    {
                        Oid = Guid.NewGuid(),
                        VoucherId = PurchaseInvoice.Oid,
                        ItemId = itemDto.InventoryItemId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount,
                        TaxAmount = itemDto.TaxAmount ?? 0,
                        TenantId = TenantId
                    };

                    Context.PurchaseInvoiceItems.Add(newItem);
                }

                // Call base create without lifecycle hooks to avoid nested transactions
                Context.PurchaseInvoices.Add(PurchaseInvoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting and contract creation
                //if (PurchaseInvoice.Status >= PurchaseInvoiceStatus.Posted)
                //{
                //    await ledgerPostingService.PostInvoiceToLedger(PurchaseInvoice);
                //    await Context.SaveChangesAsync();

                //    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(PurchaseInvoice.Oid);
                //    if (!isBalanced)
                //    {
                //        throw new InvalidOperationException($"Ledger entries for Invoice {PurchaseInvoice.Number} are not balanced");
                //    }

                //    // Auto-create Warranty/Guarantee contracts
                //    await contractAutoCreationService.ProcessInvoiceForContracts(PurchaseInvoice);
                //    await Context.SaveChangesAsync();
                //}
            }

            await transaction.CommitAsync();

            // Reload invoice with items
            var savedInvoice = await Context.PurchaseInvoices
                .Include(i => i.VoucherLines)
                .FirstOrDefaultAsync(i => i.Oid == PurchaseInvoice.Oid);

            return savedInvoice!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task HandleInvoiceItems(PurchaseInvoice invoice, List<PurchaseInvoiceItemDTO> items)
    {
        // Remove deleted items
        var deletedItemIds = items
            .Where(i => i.IsDeleted && i.Oid.HasValue)
            .Select(i => i.Oid.Value)
            .ToList();

        if (deletedItemIds.Any())
        {
            var itemsToDelete = invoice.VoucherLines.OfType<PurchaseInvoiceItem>().Where(i => deletedItemIds.Contains(i.Oid)).ToList();
            Context.PurchaseInvoiceItems.RemoveRange(itemsToDelete);
        }

        // Add or update items
        foreach (var itemDto in items.Where(i => !i.IsDeleted))
        {
            if (itemDto.IsNew)
            {
                // Add new item
                var newItem = new PurchaseInvoiceItem
                {
                    Oid = Guid.NewGuid(),
                    VoucherId = invoice.Oid,
                    ItemId = itemDto.InventoryItemId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    TaxAmount = itemDto.TaxAmount ?? 0,
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
                }
            }
        }
    }
}
