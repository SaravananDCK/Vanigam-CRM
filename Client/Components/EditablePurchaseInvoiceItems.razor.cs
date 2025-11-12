using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Security.Cryptography;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditablePurchaseInvoiceItems
{
    private RadzenNumeric<decimal> discountPercentRef;
    private RadzenNumeric<decimal> discountAmountRef;
    private Item Item { get; set; }
    [Parameter] public PurchaseInvoice PurchaseInvoice { get; set; }
    [Parameter] public string VendorState { get; set; }
    [Parameter] public string TenantAccountingState { get; set; }
    [Parameter] public List<PurchaseInvoiceItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<PurchaseInvoiceItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    private RadzenDataGrid<PurchaseInvoiceItemDTO> itemsGrid = null!;
    private PurchaseInvoiceItemDTO itemBeingEdited;

    private async Task AddNewItem()
    {
        if (PurchaseInvoice.PartyId == null)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer["Vendor is Required"] });
            return;
        }
      
        var newItem = new PurchaseInvoiceItemDTO
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

    private async Task EditRow(PurchaseInvoiceItemDTO item)
    {
        itemBeingEdited = item;
        await itemsGrid.EditRow(item);
    }

    private async Task SaveRow(PurchaseInvoiceItemDTO item)
    {
        if (item.InventoryItemId == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Failed"],
                Detail = Localizer["Purchase Invoice Item is required.."]
            });
            return;
        }
        await itemsGrid.UpdateRow(item);
        var result = Items.FirstOrDefault(i => i.InventoryItemId == null);
        if (result == null) await AddNewItem();
    }
    private async Task CancelEdit(PurchaseInvoiceItemDTO item)
    {
        itemsGrid.CancelEditRow(item);

        // If it's a new item and user cancels, remove it
        if (item.IsNew)
        {
            Items.Remove(item);
            await NotifyChanges();
        }
    }
    private async Task OnRowUpdate(PurchaseInvoiceItemDTO item)
    {
        CalculateTotal(item);
        await NotifyChanges();
    }
    private async Task DeleteItem(PurchaseInvoiceItemDTO item)
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
        PurchaseInvoice.DiscountType = args;
        await NotifyChanges();
        await Task.Delay(50);
        await CalculateDiscount();
    }
    private async Task OnInventoryItemChanged(PurchaseInvoiceItemDTO item, object value)
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
                        if (TenantAccountingState == VendorState)
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

                    if (PurchaseInvoice.DiscountType == DiscountType.Percentage)
                    {
                        await CalculateDiscount();
                    }
                    else
                    {
                        CalculateTotal(item);
                    }
                }
            }
            await NotifyChanges();
        }
    }

    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate, f => f.TaxCode.CessRate, f => f.TaxCode.CGSTRate, f => f.TaxCode.SGSTRate, f => f.TaxCode.IGSTRate)
            .Build();
    }

    private void CalculateTotal(PurchaseInvoiceItemDTO item)
    {
        if (item.TaxCodeId != null)
        {
            // Calculate tax on taxable amount (after discount)
            var taxableAmount = item.Total - item.DiscountAmount;
            double totalTaxRate;
            if (TenantAccountingState == VendorState)
            {
                totalTaxRate = item.CGSTRate + item.SGSTRate + item.CessRate;
            }
            else
            {
                totalTaxRate = item.IGSTRate + item.CessRate;
            }
            item.TaxAmount = ((decimal)totalTaxRate / 100) * taxableAmount;
        }
        PurchaseInvoice.TotalAmount = Math.Round(PurchaseInvoice.SubTotal + PurchaseInvoice.TaxAmount - PurchaseInvoice.DiscountAmount);
    }

    private async Task CalculateDiscount()
    {
        if(PurchaseInvoice.DiscountPercent > 100)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"Given Percentage: {PurchaseInvoice.DiscountPercent} is not more than 100%... "] });
            PurchaseInvoice.DiscountPercent = (PurchaseInvoice.DiscountAmount / PurchaseInvoice.SubTotal) * 100;
            return;
        }

        var items = Items.Where(i => !i.IsDeleted).ToList();
        
        if (!items.Any()) return;

        if (PurchaseInvoice.DiscountType == DiscountType.Percentage && PurchaseInvoice.DiscountPercent > 0)
        {
            PurchaseInvoice.DiscountAmount = PurchaseInvoice.SubTotal * (PurchaseInvoice.DiscountPercent / 100);
        }
        else if (PurchaseInvoice.DiscountType == DiscountType.Amount && PurchaseInvoice.DiscountAmount > 0)
        {
            PurchaseInvoice.DiscountPercent = (PurchaseInvoice.DiscountAmount / PurchaseInvoice.SubTotal) * 100;
        }

        foreach (var item in items)
        {
            item.DiscountAmount = item.Total * ((decimal)PurchaseInvoice.DiscountPercent / 100);
            CalculateTotal(item);
        }
    }

    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        await DiscountChanged.InvokeAsync(PurchaseInvoice.DiscountAmount);
        await DiscountPercentageChanged.InvokeAsync(PurchaseInvoice.DiscountPercent);
        await DiscountTypeChanged.InvokeAsync(PurchaseInvoice.DiscountType);
        StateHasChanged();
    }
}
