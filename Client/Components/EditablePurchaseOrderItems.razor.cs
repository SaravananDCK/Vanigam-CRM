using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditablePurchaseOrderItems
{
    [Parameter] public PurchaseOrder PurchaseOrder { get; set; }
    [Parameter] public List<PurchaseOrderItemDTO> Items { get; set; } = new();
    [Parameter] public EventCallback<List<PurchaseOrderItemDTO>> ItemsChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalAmountChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalTaxChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountChanged { get; set; }
    [Parameter] public EventCallback<double> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    [Parameter] public EventCallback<decimal> SubTotalChanged { get; set; }
    private RadzenDataGrid<PurchaseOrderItemDTO> itemsGrid = null!;
    private PurchaseOrderItemDTO itemBeingEdited;
    private Item Item { get; set; }
    public decimal SubTotalAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.Total) ?? 0;
    public decimal TaxAmount => Items?.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount) ?? 0;
    //public decimal DiscountAmt => Items?.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount) ?? 0;
    public double DiscountPercentage { get; set; } = 0;
    public decimal DiscountAmt { get; set; } = 0;
    public decimal GrandTotalAmount { get; set; } = 0;

    protected override void OnInitialized()
    {
        if (PurchaseOrder != null)
        {
            DiscountPercentage = (double)PurchaseOrder.DiscountPercent;
            DiscountAmt = PurchaseOrder.DiscountAmount;
            GrandTotalAmount = PurchaseOrder.TotalAmount;
        }
    }

    private async Task AddNewItem()
    {
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
        await itemsGrid.UpdateRow(item);
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
    
    private async Task OnInventoryItemChanged(Guid? itemId, PurchaseOrderItemDTO purchaseOrderItemDTO)
    {
        if (purchaseOrderItemDTO.InventoryItemId.HasValue)
        {
            if (itemId != null)
            {
                Item = await ItemApiService.GetByOid(oid: itemId.Value, expand: GetExpandString());
                if (Item != null)
                {
                    purchaseOrderItemDTO.InventoryItemName = Item.Name;
                    purchaseOrderItemDTO.UnitPrice = Item.UnitPrice;
                    purchaseOrderItemDTO.TaxCodeId = Item.TaxCodeId;

                    purchaseOrderItemDTO.TaxAmount = ((decimal)Item.TaxCode?.TaxRate / 100) * Item.UnitPrice;
                    if (PurchaseOrder.DiscountType == DiscountType.Percentage)
                    {
                        //quoteItemDTO.DiscountAmount = quoteItemDTO.Total * (decimal)DiscountPercentage / 100;
                        await CalculateDiscount((decimal)DiscountPercentage);
                    }
                    else
                    {
                        CalculateTotal(purchaseOrderItemDTO);
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

    private void CalculateTotal(PurchaseOrderItemDTO item)
    {
        // Total is calculated automatically in the DTO property
        item.TaxAmount = ((decimal)Item.TaxCode?.TaxRate / 100) * (item.Total - item.DiscountAmount);
        GrandTotalAmount = Math.Round(SubTotalAmount + TaxAmount - DiscountAmt);
    }
    private async Task CalculateDiscount(decimal discount)
    {
        var items = Items.Where(i => !i.IsDeleted);
        if (PurchaseOrder.DiscountType == DiscountType.Amount)
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
                if (PurchaseOrder.DiscountType == DiscountType.Amount)
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
        await DiscountTypeChanged.InvokeAsync(PurchaseOrder.DiscountType);
        StateHasChanged();
    }
}