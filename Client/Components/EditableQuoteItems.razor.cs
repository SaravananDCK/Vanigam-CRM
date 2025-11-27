using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditableQuoteItems
{
    [Parameter] public Quote Quote { get; set; }
    [Parameter] public int QuoteFor { get; set; }
    [Parameter] public List<QuoteItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<QuoteItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    [Parameter] public string CurrentState { get; set; }
    [Parameter] public string TenantAccountingState { get; set; }

    private RadzenDataGrid<QuoteItemDTO> itemsGrid = null!;
    private QuoteItemDTO itemBeingEdited;
    private Item Item { get; set; }

    private async Task AddNewItem()
    {
        if (Quote.PartyId == null && Quote.OpportunityId == null)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"{(QuoteFor == 0 ? "Vendor" : "Opportunity") } is Required"] });
            return;
        }
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
        if (item.InventoryItemId == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Failed"],
                Detail = Localizer["Item is required.."]
            });
            return;
        }
        await itemsGrid.UpdateRow(item);
        var result = Items.FirstOrDefault(i => i.InventoryItemId == null);
        if (result == null) await AddNewItem();
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
    private async Task OnDiscountTypeChange(DiscountType args)
    {
        Quote.DiscountType = args;
        await DiscountTypeChanged.InvokeAsync(Quote.DiscountType);
        StateHasChanged();
    }
    
    private async Task OnInventoryItemChanged(object value, QuoteItemDTO item)
    {
        if (value is Guid inventoryItemId)
        {
            item.InventoryItemId = inventoryItemId;
            if (value is Guid id)
            {
                Item = await ItemApiService.GetByOid(oid: id, expand: GetExpandString());
                if (Item != null)
                {
                    item.InventoryItemName = Item.Name;
                    item.UnitPrice = Item.UnitPrice;
                    item.TaxCodeId = Item.TaxCodeId;

                    if (Item.TaxCode != null)
                    {
                        if (TenantAccountingState == CurrentState)
                        {
                            item.CGSTRate = Item.TaxCode.CGSTRate;
                            item.SGSTRate = Item.TaxCode.SGSTRate;
                            item.IGSTRate = 0;
                        }
                        else
                        {
                            item.CGSTRate = 0;
                            item.SGSTRate = 0;
                            item.IGSTRate = Item.TaxCode.IGSTRate;
                        }
                        item.CessRate = Item.TaxCode.CessRate;

                        // Total tax is the sum of all GST components
                        var totalTaxRate = item.CGSTRate + item.SGSTRate + item.IGSTRate + item.CessRate;
                        item.TaxAmount = ((decimal)totalTaxRate / 100) * Item.UnitPrice;
                    }
                }
                await CalculateItemAmount(item);
            }
        }
    }

    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate, f => f.TaxCode.CessRate, f => f.TaxCode.CGSTRate, f => f.TaxCode.SGSTRate, f => f.TaxCode.IGSTRate)
            .Build();
    }
    private async Task CalculateItemAmount(QuoteItemDTO item)
    {
        if (Quote.DiscountAmount > 0 || Quote.DiscountPercent > 0)
        {
            await CalculateDiscount();
        }
        CalculateTotal(item);
        await NotifyChanges();
    }
    private void CalculateTotal(QuoteItemDTO item)
    {
        if (item.TaxCodeId != null)
        {
            item.DiscountAmount = item.Total * (Quote.DiscountPercent / 100);

            var taxableAmount = item.Total - item.DiscountAmount;
            double totalTaxRate;
            if (TenantAccountingState == CurrentState)
            {
                totalTaxRate = item.CGSTRate + item.SGSTRate + item.CessRate;
            }
            else
            {
                totalTaxRate = item.IGSTRate + item.CessRate;
            }
            item.TaxAmount = ((decimal)totalTaxRate / 100) * taxableAmount;
        }
        Quote.TotalAmount = Math.Round(Quote.SubTotal + Quote.TaxAmount - Quote.DiscountAmount);
    }
    private async Task CalculateDiscount(bool isCalulateItems = false)
    {
        if (Quote.DiscountPercent == 0 && Quote.DiscountAmount == 0) return;
        if (Quote.DiscountPercent > 100)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"Given Percentage: {Quote.DiscountPercent} is not more than 100%... "] });
            Quote.DiscountPercent = (Quote.DiscountAmount / Quote.SubTotal) * 100;
            return;
        }

        var items = Items.Where(i => !i.IsDeleted).ToList();

        if (!items.Any()) return;

        if (Quote.DiscountType == DiscountType.Percentage)
        {
            Quote.DiscountAmount = Quote.SubTotal * (Quote.DiscountPercent / 100);
        }
        else if (Quote.DiscountType == DiscountType.Amount)
        {
            Quote.DiscountPercent = (Quote.DiscountAmount / Quote.SubTotal) * 100;
        }

        if (isCalulateItems)
        {
            foreach (var item in items)
            {
                CalculateTotal(item);
            }
        }
        if (Quote.DiscountPercent > 0) await DiscountPercentageChanged.InvokeAsync(Quote.DiscountPercent);
    }
    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        StateHasChanged();
    }
}