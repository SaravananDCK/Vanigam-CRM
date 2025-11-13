using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditablePurchaseOrderItems
{
    private RadzenNumeric<decimal> discountPercentRef;
    private RadzenNumeric<decimal> discountAmountRef;
    private Item Item { get; set; }
    [Parameter] public PurchaseOrder PurchaseOrder { get; set; }
    [Parameter] public string VendorState { get; set; }
    [Parameter] public string TenantAccountingState { get; set; }
    [Parameter] public List<PurchaseOrderItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<PurchaseOrderItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    private RadzenDataGrid<PurchaseOrderItemDTO> itemsGrid = null!;
    private PurchaseOrderItemDTO itemBeingEdited;
    
    private async Task AddNewItem()
    {
        if (PurchaseOrder.PartyId == null)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer["Vendor is Required"] });
            return;
        }
        var newItem = new PurchaseOrderItemDTO
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

    private async Task EditRow(PurchaseOrderItemDTO item)
    {
        itemBeingEdited = item;
        await itemsGrid.EditRow(item);
    }

    private async Task SaveRow(PurchaseOrderItemDTO item)
    {
        if (item.InventoryItemId == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Failed"],
                Detail = Localizer["Purchase Order Item is required.."]
            });
            return;
        }
        await itemsGrid.UpdateRow(item);
        var result = Items.FirstOrDefault(i => i.InventoryItemId == null);
        if (result == null) await AddNewItem();
    }

    private async Task CancelEdit(PurchaseOrderItemDTO item)
    {
        itemsGrid.CancelEditRow(item);

        // If it's a new item and user cancels, remove it
        if (item.IsNew)
        {
            Items.Remove(item);
            await NotifyChanges();
        }
    }

    private async Task OnRowUpdate(PurchaseOrderItemDTO item)
    {
        CalculateTotal(item);
        await NotifyChanges();
    }

    private async Task DeleteItem(PurchaseOrderItemDTO item)
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
        PurchaseOrder.DiscountType = args;
        await DiscountTypeChanged.InvokeAsync(PurchaseOrder.DiscountType);
        StateHasChanged();
    }
    
    private async Task OnInventoryItemChanged(object value, PurchaseOrderItemDTO item)
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

    private async Task CalculateItemAmount(PurchaseOrderItemDTO item)
    {
        if (PurchaseOrder.DiscountAmount > 0 || PurchaseOrder.DiscountPercent > 0)
        {
            await CalculateDiscount();
        }
        CalculateTotal(item);
        await NotifyChanges();
    }

    private void CalculateTotal(PurchaseOrderItemDTO item)
    {
        if (item.TaxCodeId != null)
        {
            item.DiscountAmount = item.Total * ((decimal)PurchaseOrder.DiscountPercent / 100);
            
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
        PurchaseOrder.TotalAmount = Math.Round(PurchaseOrder.SubTotal + PurchaseOrder.TaxAmount - PurchaseOrder.DiscountAmount);
    }
    private async Task CalculateDiscount(bool isCalulateItems = false)
    {
        if (PurchaseOrder.DiscountPercent == 0 && PurchaseOrder.DiscountAmount == 0) return;
        if (PurchaseOrder.DiscountPercent > 100)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"Given Percentage: {PurchaseOrder.DiscountPercent} is not more than 100%... "] });
            PurchaseOrder.DiscountPercent = (PurchaseOrder.DiscountAmount / PurchaseOrder.SubTotal) * 100;
            return;
        }

        var items = Items.Where(i => !i.IsDeleted).ToList();

        if (!items.Any()) return;

        if (PurchaseOrder.DiscountType == DiscountType.Percentage)
        {
            PurchaseOrder.DiscountAmount = PurchaseOrder.SubTotal * (PurchaseOrder.DiscountPercent / 100);
        }
        else if (PurchaseOrder.DiscountType == DiscountType.Amount)
        {
            PurchaseOrder.DiscountPercent = (PurchaseOrder.DiscountAmount / PurchaseOrder.SubTotal) * 100;
        }

        if (isCalulateItems)
        {
            foreach (var item in items)
            {
                CalculateTotal(item);
            }
        }
        //if (PurchaseOrder.DiscountAmount > 0) await DiscountChanged.InvokeAsync(PurchaseOrder.DiscountAmount);
        if (PurchaseOrder.DiscountPercent > 0) await DiscountPercentageChanged.InvokeAsync(PurchaseOrder.DiscountPercent);
    }
    
    private async Task NotifyChanges()
    {
        await ItemsChanged.InvokeAsync(Items);
        StateHasChanged();
    }
}