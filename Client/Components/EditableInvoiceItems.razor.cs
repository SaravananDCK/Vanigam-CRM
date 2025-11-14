using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditableInvoiceItems
{
    private Item Item { get; set; }
    [Parameter] public Invoice Invoice { get; set; }
    [Parameter] public string CurrentState { get; set; }
    [Parameter] public string TenantAccountingState { get; set; }
    [Parameter] public List<InvoiceItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<InvoiceItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    private RadzenDataGrid<InvoiceItemDTO> itemsGrid = null!;
    private InvoiceItemDTO itemBeingEdited;

    private async Task AddNewItem()
    {
        if (Invoice.PartyId == null)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer["Customer is Required"] });
            return;
        }
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
        if (item.InventoryItemId == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Failed"],
                Detail = Localizer["Invoice Item is required.."]
            });
            return;
        }
        await itemsGrid.UpdateRow(item);
        var result = Items.FirstOrDefault(i => i.InventoryItemId == null);
        if (result == null) await AddNewItem();
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
        CalculateTotal(item);
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
    private async Task OnDiscountTypeChange(DiscountType args)
    {
        Invoice.DiscountType = args;
        await DiscountTypeChanged.InvokeAsync(Invoice.DiscountType);
        StateHasChanged();
    }
    private async Task OnInventoryItemChanged(InvoiceItemDTO item, object value)
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
                    // Calculate GST breakdown based on TaxCode rates
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
                    await CalculateItemAmount(item);
                }
            }
        }
    }
    private async Task CalculateItemAmount(InvoiceItemDTO item)
    {
        if (Invoice.DiscountAmount > 0 || Invoice.DiscountPercent > 0)
        {
            await CalculateDiscount();
        }
        CalculateTotal(item);
        await NotifyChanges();
    }
    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate, f => f.TaxCode.CessRate, f => f.TaxCode.CGSTRate, f => f.TaxCode.SGSTRate, f => f.TaxCode.IGSTRate)
            .Build();
    }
    private void CalculateTotal(InvoiceItemDTO item)
    {
        if (item.TaxCodeId != null)
        {
            item.DiscountAmount = item.Total * (Invoice.DiscountPercent / 100);
            // Calculate tax on taxable amount (after discount)
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
        Invoice.TotalAmount = Math.Round(Invoice.SubTotal + Invoice.TaxAmount - Invoice.DiscountAmount);
    }
    private async Task CalculateDiscount(bool isCalulateItems = false)
    {
        if (Invoice.DiscountPercent > 100)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"Given Percentage: {Invoice.DiscountPercent} is not more than 100%... "] });
            Invoice.DiscountPercent = (Invoice.DiscountAmount / Invoice.SubTotal) * 100;
            return;
        }

        var items = Items.Where(i => !i.IsDeleted).ToList();

        if (!items.Any()) return;

        if (Invoice.DiscountType == DiscountType.Percentage && Invoice.DiscountPercent > 0)
        {
            Invoice.DiscountAmount = Invoice.SubTotal * (Invoice.DiscountPercent / 100);
        }
        else if (Invoice.DiscountType == DiscountType.Amount && Invoice.DiscountAmount > 0)
        {
            Invoice.DiscountPercent = (Invoice.DiscountAmount / Invoice.SubTotal) * 100;
        }
        if (isCalulateItems)
        {
            foreach (var item in items)
            {
                item.DiscountAmount = item.Total * (Invoice.DiscountPercent / 100);
                CalculateTotal(item);
            }
        }
        if (Invoice.DiscountPercent > 0) await DiscountPercentageChanged.InvokeAsync(Invoice.DiscountPercent);
    }
    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        StateHasChanged();
    }
}