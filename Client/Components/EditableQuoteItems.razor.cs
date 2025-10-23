using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditableQuoteItems
{
    [Parameter] public Quote Quote { get; set; }
    [Parameter] public List<QuoteItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<QuoteItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalAmountChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalTaxChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountChanged { get; set; }
    [Parameter] public EventCallback<double> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    [Parameter] public EventCallback<decimal> SubTotalChanged { get; set; }
    private RadzenDataGrid<QuoteItemDTO> itemsGrid = null!;
    private QuoteItemDTO itemBeingEdited;
    public decimal SubTotalAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.Total) ?? 0;
    public decimal TaxAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount) ?? 0;
    //public decimal DiscountAmt => Items?.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount) ?? 0;
    public double DiscountPercentage { get; set; } = 0;
    public decimal DiscountAmt { get; set; } = 0;
    public decimal GrandTotalAmount { get; set; } = 0;

    protected override void OnInitialized()
    {
        if (Quote != null)
        {
            DiscountPercentage = (double)Quote.DiscountPercent;
            DiscountAmt = Quote.DiscountAmount;
            GrandTotalAmount = Quote.TotalAmount;
        }
    }

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
    private Item Item { get; set; }
    private async Task OnInventoryItemChanged(Guid? itemId, QuoteItemDTO quoteItemDTO)
    {
        if (quoteItemDTO.InventoryItemId.HasValue)
        {
            if (itemId != null)
            {
                Item = await ItemApiService.GetByOid(oid: itemId.Value, expand: GetExpandString());
                if (Item != null)
                {
                    quoteItemDTO.InventoryItemName = Item.Name;
                    quoteItemDTO.UnitPrice = Item.UnitPrice;
                    quoteItemDTO.TaxCodeId = Item.TaxCodeId;

                    quoteItemDTO.TaxAmount = ((decimal)Item.TaxCode?.TaxRate / 100) * Item.UnitPrice;
                    if (Quote.DiscountType == DiscountType.Percentage)
                    {
                        //quoteItemDTO.DiscountAmount = quoteItemDTO.Total * (decimal)DiscountPercentage / 100;
                        await CalculateDiscount((decimal)DiscountPercentage);
                    }
                    else
                    {
                        CalculateTotal(quoteItemDTO);
                    }
                    // You might want to set default price from inventory item if available
                }
            }
        }
        await NotifyChanges();
    }

    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate)
            .Build();
    }

    private void CalculateTotal(QuoteItemDTO item)
    {
        // Total is calculated automatically in the DTO property
        item.TaxAmount = ((decimal)Item.TaxCode?.TaxRate / 100) * (item.Total - item.DiscountAmount);
        GrandTotalAmount = Math.Round(SubTotalAmount + TaxAmount - DiscountAmt);
    }
    private async Task CalculateDiscount(decimal discount)
    {
        var items = Items.Where(i => !i.IsDeleted);
        if (Quote.DiscountType == DiscountType.Amount)
        {
            DiscountPercentage = 0;
        }
        if (DiscountPercentage != 0)
        {
            DiscountAmt = SubTotalAmount * (discount / 100);
        }
        if (items.Any())
        {
            var itemCounts = items.Count();
            foreach (var item in items)
            {
                if (Quote.DiscountType == DiscountType.Amount)
                {
                    item.DiscountAmount = DiscountAmt / itemCounts;
                }
                else
                {
                    item.DiscountAmount = item.Total * ((decimal)DiscountPercentage / 100);
                }
                CalculateTotal(item);
            }
        }
        await NotifyChanges();
    }
    private async Task NotifyChanges()
    {
        await SubTotalChanged.InvokeAsync(SubTotalAmount);
        await ItemsChanged.InvokeAsync(Items);
        await TotalTaxChanged.InvokeAsync(TaxAmount);
        await DiscountChanged.InvokeAsync(DiscountAmt);
        await DiscountPercentageChanged.InvokeAsync(DiscountPercentage);
        await TotalAmountChanged.InvokeAsync(GrandTotalAmount);
        await DiscountTypeChanged.InvokeAsync(Quote.DiscountType);
        StateHasChanged();
    }
}