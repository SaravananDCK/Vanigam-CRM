using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InvoiceService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Invoice>> logger,
    LedgerPostingService ledgerPostingService,
    ContractAutoCreationService contractAutoCreationService)
    : BaseService<Invoice>(context, logger)
{
    public override DbSet<Invoice> GetDbSet()
    {
        return Context.Invoices;
    }

    /// <summary>
    /// Creates a new invoice and posts it to the ledger if status is Posted.
    /// </summary>
    protected override async Task OnCreatedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            await base.OnCreatedAsync(invoice);

            // Post to ledger if invoice is already in Posted status
            if (invoice.Status >= InvoiceStatus.Posted)
            {
                await ledgerPostingService.PostInvoiceToLedger(invoice);
                await Context.SaveChangesAsync();

                // Validate entries balance
                var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                if (!isBalanced)
                {
                    throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
                }

                // Auto-create Warranty/Guarantee contracts
                await contractAutoCreationService.ProcessInvoiceForContracts(invoice);
                await Context.SaveChangesAsync();
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
    /// Updates an invoice. If status changes to Posted, creates ledger entries.
    /// If status changes from Posted to another status, reverses ledger entries.
    /// </summary>
    protected override async Task OnUpdatedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the original invoice to check status changes
            var original = await Context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Oid == invoice.Oid);

            await base.OnUpdatedAsync(invoice);

            // Handle status changes
            if (original != null)
            {
                // Status changed from non-Posted to Posted
                if (original.Status != InvoiceStatus.Posted && invoice.Status >= InvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostInvoiceToLedger(invoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
                    }

                    // Auto-create Warranty/Guarantee contracts
                    await contractAutoCreationService.ProcessInvoiceForContracts(invoice);
                    await Context.SaveChangesAsync();
                }
                // Status changed from Posted to non-Posted (reversal)
                else if (original.Status >= InvoiceStatus.Posted && invoice.Status < InvoiceStatus.Posted)
                {
                    await ledgerPostingService.ReverseVoucherEntries(invoice.Oid, $"Invoice {invoice.Number} status changed to {invoice.Status}");
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
    /// Deletes an invoice. If it was posted, reverses the ledger entries first.
    /// </summary>
    protected override async Task<bool> OnDeletedAsync(Invoice invoice)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            if (invoice == null)
                return false;

            // Reverse ledger entries if invoice was posted
            if (invoice.Status >= InvoiceStatus.Posted)
            {
                await ledgerPostingService.ReverseVoucherEntries(invoice.Oid, $"Invoice {invoice.Number} deleted");
                await Context.SaveChangesAsync();
            }

            var deleted = await base.DeleteAsync(invoice);
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
    /// Bulk save invoice with items. Handles create/update of invoice and its items,
    /// and automatically posts to ledger if invoice status is Posted.
    /// This method ensures all lifecycle hooks are triggered for proper ledger posting.
    /// </summary>
    public async Task<Invoice> BulkSaveInvoiceWithItems(InvoiceBulkSaveDTO invoiceData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Invoice invoice;
            bool isUpdate = invoiceData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing invoice
                invoice = await Context.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Oid == invoiceData.Oid.Value);

                if (invoice == null)
                    throw new InvalidOperationException("Invoice not found");

                // Update invoice properties
                invoice.Number = invoiceData.Number;
                invoice.Status = invoiceData.Status;
                invoice.PartyId = invoiceData.PartyId;
                invoice.TotalAmount = invoiceData.TotalAmount;
                invoice.SubTotal = invoiceData.SubTotal;
                invoice.TaxAmount = invoiceData.TaxAmount;
                invoice.VoucherDate = invoiceData.VoucherDate;

                // Handle invoice items
                await HandleInvoiceItems(invoice, invoiceData.Items);

                // Call base update without lifecycle hooks to avoid nested transactions
                Context.Invoices.Update(invoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting and contract creation
                if (invoice.Status >= InvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostInvoiceToLedger(invoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
                    }

                    // Auto-create Warranty/Guarantee contracts
                    await contractAutoCreationService.ProcessInvoiceForContracts(invoice);
                    await Context.SaveChangesAsync();
                }
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
                    SubTotal = invoiceData.SubTotal,
                    TaxAmount = invoiceData.TaxAmount,
                    VoucherDate = invoiceData.VoucherDate,
                    TenantId = TenantId
                };

                // Add invoice items
                foreach (var itemDto in invoiceData.Items.Where(i => !i.IsDeleted))
                {
                    var newItem = new InvoiceItem
                    {
                        Oid = Guid.NewGuid(),
                        VoucherId = invoice.Oid,
                        ItemId = itemDto.InventoryItemId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount ?? 0,
                        TaxAmount = itemDto.TaxAmount ?? 0,
                        TenantId = TenantId
                    };

                    Context.InvoiceItems.Add(newItem);
                }

                // Call base create without lifecycle hooks to avoid nested transactions
                Context.Invoices.Add(invoice);
                await Context.SaveChangesAsync();

                // Manually trigger ledger posting and contract creation
                if (invoice.Status >= InvoiceStatus.Posted)
                {
                    await ledgerPostingService.PostInvoiceToLedger(invoice);
                    await Context.SaveChangesAsync();

                    var isBalanced = await ledgerPostingService.ValidateVoucherEntriesBalance(invoice.Oid);
                    if (!isBalanced)
                    {
                        throw new InvalidOperationException($"Ledger entries for Invoice {invoice.Number} are not balanced");
                    }

                    // Auto-create Warranty/Guarantee contracts
                    await contractAutoCreationService.ProcessInvoiceForContracts(invoice);
                    await Context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            // Reload invoice with items
            var savedInvoice = await Context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Oid == invoice.Oid);

            return savedInvoice!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles adding, updating, and deleting invoice items
    /// </summary>
    private async Task HandleInvoiceItems(Invoice invoice, List<InvoiceItemDTO> items)
    {
        // Remove deleted items
        var deletedItemIds = items
            .Where(i => i.IsDeleted && i.Oid.HasValue)
            .Select(i => i.Oid.Value)
            .ToList();

        if (deletedItemIds.Any())
        {
            var itemsToDelete = invoice.Items.Where(i => deletedItemIds.Contains(i.Oid)).ToList();
            Context.InvoiceItems.RemoveRange(itemsToDelete);
        }

        // Add or update items
        foreach (var itemDto in items.Where(i => !i.IsDeleted))
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
                    DiscountAmount = itemDto.DiscountAmount ?? 0,
                    TaxAmount = itemDto.TaxAmount ?? 0,
                    TenantId = TenantId
                };

                Context.InvoiceItems.Add(newItem);
            }
            else if (itemDto.Oid.HasValue)
            {
                // Update existing item
                var existingItem = await Context.InvoiceItems
                    .FirstOrDefaultAsync(i => i.Oid == itemDto.Oid.Value);

                if (existingItem != null)
                {
                    existingItem.ItemId = itemDto.InventoryItemId;
                    existingItem.Quantity = itemDto.Quantity;
                    existingItem.UnitPrice = itemDto.UnitPrice;
                    existingItem.DiscountAmount = itemDto.DiscountAmount ?? 0;
                    existingItem.TaxAmount = itemDto.TaxAmount ?? 0;
                }
            }
        }
    }
}
