using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components;

public partial class EditableQuoteItems
{
    [Parameter] public List<QuoteItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<QuoteItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalAmountChanged { get; set; }
    private RadzenDataGrid<QuoteItemDTO> itemsGrid = null!;
    private QuoteItemDTO itemBeingEdited;
    public decimal TotalAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.Total) ?? 0;

    private async Task AddNewItem()
    {
        var newItem = new QuoteItemDTO
        {
            Quantity = 1,
            UnitPrice = 0,
        };

        Items.Add(newItem);
        await NotifyChanges();

        // Start editing the new item immediately
        await Task.Delay(100); // Small delay to ensure the grid is updated
        await EditRow(newItem);
    }

    private async Task EditRow(QuoteItemDTO item)
    {
        itemBeingEdited = item;
        await itemsGrid.EditRow(item);
    }

    private async Task SaveRow(QuoteItemDTO item)
    {
        await itemsGrid.UpdateRow(item);
    }

    private async Task CancelEdit(QuoteItemDTO item)
    {
        itemsGrid.CancelEditRow(item);

        // If it's a new item and user cancels, remove it
        if (item.IsNew)
        {
            Items.Remove(item);
            await NotifyChanges();
        }
    }

    private async Task OnRowUpdate(QuoteItemDTO item)
    {
        CalculateTotal(item);
        await NotifyChanges();
    }

    private async Task DeleteItem(QuoteItemDTO item)
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

    private async Task OnInventoryItemChanged(Guid? itemId, QuoteItemDTO quoteItemDTO)
    {
        if (quoteItemDTO.InventoryItemId.HasValue)
        {
            if (itemId!=null)
            {
                var item = await InventoryItemApiService.GetByOid(oid: itemId.Value);
                if (item != null)
                {
                    quoteItemDTO.InventoryItemName = item.Name;
                    quoteItemDTO.UnitPrice = item.UnitPrice;
                    quoteItemDTO.TaxCodeId = item.TaxCodeId;
                    quoteItemDTO.TaxAmount = (item.TaxCode?.TaxRate ?? 0) * item.UnitPrice;
                    // You might want to set default price from inventory item if available
                }
            }
        }

        CalculateTotal(quoteItemDTO);
        await NotifyChanges();
    }

    private void CalculateTotal(QuoteItemDTO item)
    {
        // Total is calculated automatically in the DTO property
    }

    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        await TotalAmountChanged.InvokeAsync(TotalAmount);
        StateHasChanged();
    }
}