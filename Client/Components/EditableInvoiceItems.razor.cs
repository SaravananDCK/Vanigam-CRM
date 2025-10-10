using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client.Components;

public partial class EditableInvoiceItems
{
    [Parameter] public List<InvoiceItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<InvoiceItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalAmountChanged { get; set; }
    private RadzenDataGrid<InvoiceItemDTO> itemsGrid = null!;
    private InvoiceItemDTO itemBeingEdited;
    public decimal TotalAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.Total) ?? 0;

    private async Task AddNewItem()
    {
        var newItem = new InvoiceItemDTO
        {
            Quantity = 1,
            UnitPrice = 0
        };

        Items.Add(newItem);
        await NotifyChanges();

        // Start editing the new item immediately
        await Task.Delay(100); // Small delay to ensure the grid is updated
        await EditRow(newItem);
    }

    private async Task EditRow(InvoiceItemDTO item)
    {
        itemBeingEdited = item;
        await itemsGrid.EditRow(item);
    }

    private async Task SaveRow(InvoiceItemDTO item)
    {
        await itemsGrid.UpdateRow(item);
    }

    private async Task CancelEdit(InvoiceItemDTO item)
    {
        itemsGrid.CancelEditRow(item);

        // If it's a new item and user cancels, remove it
        if (item.IsNew)
        {
            Items.Remove(item);
            await NotifyChanges();
        }
    }

    private async Task OnRowUpdate(InvoiceItemDTO item)
    {
        await NotifyChanges();
    }

    private async Task DeleteItem(InvoiceItemDTO item)
    {
        if (item.IsNew)
        {
            Items.Remove(item);
        }
        else
        {
            item.IsDeleted = true;
        }

        await NotifyChanges();
    }

    private async Task OnInventoryItemChanged(InvoiceItemDTO item, object value)
    {
        if (value is Guid inventoryItemId)
        {
            item.InventoryItemId = inventoryItemId;

            if (value is Guid id)
            {
                var inventoryItem = await ItemApiService.GetByOid(oid: id);
                if (inventoryItem != null)
                {
                    item.InventoryItemName = inventoryItem.Name;
                    item.UnitPrice = inventoryItem.UnitPrice;
                    
                }
            }

            await NotifyChanges();
        }
    }

    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        await TotalAmountChanged.InvokeAsync(TotalAmount);
        StateHasChanged();
    }
}