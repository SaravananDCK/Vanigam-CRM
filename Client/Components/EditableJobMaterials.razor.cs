using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditableJobMaterials
{
    private Item Item { get; set; }
    [Parameter] public Job Job { get; set; }
    [Parameter] public List<MaterialUsageDTO> Materials { get; set; } = new();
    [Parameter] public EventCallback<List<MaterialUsageDTO>> MaterialsChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalAmountChanged { get; set; }
    [Parameter] public EventCallback<decimal> TotalTaxChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountChanged { get; set; }
    [Parameter] public EventCallback<double> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    [Parameter] public EventCallback<decimal> SubTotalChanged { get; set; }
    private RadzenDataGrid<MaterialUsageDTO> materialsGrid = null!;
    private MaterialUsageDTO materialBeingEdited;
    public decimal SubTotalAmount => Materials?.Where(m => !m.IsDeleted).Sum(m => m.Total) ?? 0;
    public decimal TaxAmount => Materials?.Where(m => !m.IsDeleted).Sum(m => m.TaxAmount) ?? 0;
    public decimal TotalAmount => Materials?.Where(m => !m.IsDeleted).Sum(m => m.Total) ?? 0;
    public double DiscountPercentage { get; set; } = 0;
    public decimal DiscountAmt { get; set; } = 0;
    public decimal GrandTotalAmount { get; set; } = 0;

    protected override void OnInitialized()
    {
        if (Job != null)
        {
            DiscountPercentage = (double)Job.DiscountPercent;
            DiscountAmt = Job.DiscountAmount;
            GrandTotalAmount = Job.TotalAmount;
        }
    }

    private async Task AddNewMaterial()
    {
        var newMaterial = new MaterialUsageDTO
        {
            Quantity = 1,
            UnitPrice = 0
        };

        Materials.Add(newMaterial);
        await NotifyChanges();

        // Start editing the new material immediately
        await Task.Delay(100); // Small delay to ensure the grid is updated
        await EditRow(newMaterial);
    }

    private async Task EditRow(MaterialUsageDTO material)
    {
        materialBeingEdited = material;
        await materialsGrid.EditRow(material);
    }

    private async Task SaveRow(MaterialUsageDTO material)
    {
        await materialsGrid.UpdateRow(material);
    }

    private async Task CancelEdit(MaterialUsageDTO material)
    {
        materialsGrid.CancelEditRow(material);

        // If it's a new material and user cancels, remove it
        if (material.IsNew)
        {
            Materials.Remove(material);
            await NotifyChanges();
        }
    }

    private async Task OnRowUpdate(MaterialUsageDTO material)
    {
        CalculateTotal(material);
        await NotifyChanges();
    }

    private async Task DeleteMaterial(MaterialUsageDTO material)
    {
        if (material.IsNew)
        {
            Materials.Remove(material);
        }
        else
        {
            material.IsDeleted = true;
        }

        await NotifyChanges();
    }

    private async Task OnInventoryItemChanged(MaterialUsageDTO material, object value)
    {
        if (value is Guid inventoryItemId)
        {
            material.InventoryItemId = inventoryItemId;

            if (value is Guid id)
            {
                Item = await ItemApiService.GetByOid(oid: id, expand: GetExpandString());
                if (Item != null)
                {
                    material.InventoryItemName = Item.Name;
                    material.UnitPrice = Item.UnitPrice;
                    material.TaxCodeId = Item.TaxCodeId;

                    // Calculate GST breakdown based on TaxCode rates
                    if (Item.TaxCode != null)
                    {
                        material.CGSTRate = Item.TaxCode.CGSTRate;
                        material.SGSTRate = Item.TaxCode.SGSTRate;
                        material.IGSTRate = Item.TaxCode.IGSTRate;
                        material.CessRate = Item.TaxCode.CessRate;

                        // Total tax is the sum of all GST components
                        var totalTaxRate = Item.TaxCode.CGSTRate + Item.TaxCode.SGSTRate +
                                         Item.TaxCode.IGSTRate + Item.TaxCode.CessRate;
                        material.TaxAmount = ((decimal)totalTaxRate / 100) * Item.UnitPrice;
                    }

                    if (Job.DiscountType == DiscountType.Percentage)
                    {
                        await CalculateDiscount((decimal)DiscountPercentage);
                    }
                    else
                    {
                        CalculateTotal(material);
                    }
                }
            }

            await NotifyChanges();
        }
    }

    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate)
            .Build();
    }

    private void CalculateTotal(MaterialUsageDTO material)
    {
        // Total is calculated automatically in the DTO property
        if (Item?.TaxCode != null)
        {
            // Calculate tax on taxable amount (after discount)
            var taxableAmount = material.Total - material.DiscountAmount;
            var totalTaxRate = Item.TaxCode.CGSTRate + Item.TaxCode.SGSTRate +
                             Item.TaxCode.IGSTRate + Item.TaxCode.CessRate;
            material.TaxAmount = ((decimal)totalTaxRate / 100) * taxableAmount;
        }
        GrandTotalAmount = Math.Round(SubTotalAmount + TaxAmount - DiscountAmt);
    }

    private async Task CalculateDiscount(decimal discount)
    {
        var materials = Materials.Where(m => !m.IsDeleted);
        if (Job.DiscountType == DiscountType.Amount)
        {
            DiscountPercentage = 0;
        }
        if (DiscountPercentage != 0)
        {
            DiscountAmt = SubTotalAmount * (discount / 100);
        }
        if (materials.Any())
        {
            var materialCounts = materials.Count();
            foreach (var material in materials)
            {
                if (Job.DiscountType == DiscountType.Amount)
                {
                    material.DiscountAmount = DiscountAmt / materialCounts;
                }
                else
                {
                    material.DiscountAmount = material.Total * ((decimal)DiscountPercentage / 100);
                }
                CalculateTotal(material);
            }
        }
        await NotifyChanges();
    }

    private async Task NotifyChanges()
    {
        await SubTotalChanged.InvokeAsync(SubTotalAmount);
        await MaterialsChanged.InvokeAsync(Materials);
        await TotalTaxChanged.InvokeAsync(TaxAmount);
        await DiscountChanged.InvokeAsync(DiscountAmt);
        await DiscountPercentageChanged.InvokeAsync(DiscountPercentage);
        await TotalAmountChanged.InvokeAsync(GrandTotalAmount);
        await DiscountTypeChanged.InvokeAsync(Job.DiscountType);
        StateHasChanged();
    }
}
