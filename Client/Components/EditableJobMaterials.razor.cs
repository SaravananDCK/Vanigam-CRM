using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Components;

public partial class EditableJobMaterials
{
    private Item Item { get; set; }
    [Parameter] public Job Job { get; set; }
    [Parameter] public string CustomerState { get; set; }
    [Parameter] public string TenantAccountingState { get; set; }
    [Parameter] public List<MaterialUsageDTO> Materials { get; set; } = new();
    [Parameter] public EventCallback<List<MaterialUsageDTO>> MaterialsChanged { get; set; }
    [Parameter] public EventCallback<decimal> DiscountPercentageChanged { get; set; }
    [Parameter] public EventCallback<DiscountType> DiscountTypeChanged { get; set; }
    private RadzenDataGrid<MaterialUsageDTO> materialsGrid = null!;
    private MaterialUsageDTO materialBeingEdited;
    private async Task AddNewMaterial()
    {
        if (Job.PartyId == null)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer["Customer is Required"] });
            return;
        }
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
        if (material.InventoryItemId == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Failed"],
                Detail = Localizer["Material Item is required.."]
            });
            return;
        }

        await materialsGrid.UpdateRow(material);
        var result = Materials.FirstOrDefault(i => i.InventoryItemId == null);
        if (result == null) await AddNewMaterial();
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
    private async Task OnDiscountTypeChange(DiscountType args)
    {
        Job.DiscountType = args;
        await DiscountTypeChanged.InvokeAsync(Job.DiscountType);
        StateHasChanged();
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
                        if (TenantAccountingState == CustomerState)
                        {
                            material.CGSTRate = Item.TaxCode.CGSTRate;
                            material.SGSTRate = Item.TaxCode.SGSTRate;
                            material.IGSTRate = 0;
                        }
                        else
                        {
                            material.CGSTRate = 0;
                            material.SGSTRate = 0;
                            material.IGSTRate = Item.TaxCode.IGSTRate;
                        }
                        material.CessRate = Item.TaxCode.CessRate;

                        // Total tax is the sum of all GST components
                        var totalTaxRate = Item.TaxCode.CGSTRate + Item.TaxCode.SGSTRate +
                                         Item.TaxCode.IGSTRate + Item.TaxCode.CessRate;
                        material.TaxAmount = ((decimal)totalTaxRate / 100) * Item.UnitPrice;
                    }
                    await CalculateMaterialAmount(material);
                }
            }
        }
    }

    protected string GetExpandString()
    {
        return new ODataExpand<Item>()
            .Expand(f => f.TaxCode, f => f.TaxCode.TaxRate, f => f.TaxCode.CessRate, f => f.TaxCode.CGSTRate, f => f.TaxCode.SGSTRate, f => f.TaxCode.IGSTRate)
            .Build();
    }
    private async Task CalculateMaterialAmount(MaterialUsageDTO material)
    {
        if (Job.DiscountAmount > 0 || Job.DiscountPercent > 0)
        {
            await CalculateDiscount();
        }
        CalculateTotal(material);
        await NotifyChanges();
    }
    private void CalculateTotal(MaterialUsageDTO material)
    {
        if (material.TaxCodeId != null)
        {
            material.DiscountAmount = material.Total * (Job.DiscountPercent / 100);

            var taxableAmount = material.Total - material.DiscountAmount;
            double totalTaxRate;
            if (TenantAccountingState == CustomerState)
            {
                totalTaxRate = material.CGSTRate + material.SGSTRate + material.CessRate;
            }
            else
            {
                totalTaxRate = material.IGSTRate + material.CessRate;
            }
            material.TaxAmount = ((decimal)totalTaxRate / 100) * taxableAmount;
        }
        Job.TotalAmount = Math.Round(Job.SubTotal + Job.TaxAmount - Job.DiscountAmount);
    }

    private async Task CalculateDiscount(bool isCalulateMaterials = false)
    {
        if (Job.DiscountPercent == 0 && Job.DiscountAmount == 0) return;
        if (Job.DiscountPercent > 100)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = Localizer["Error"], Detail = Localizer[$"Given Percentage: {Job.DiscountPercent} is not more than 100%... "] });
            Job.DiscountPercent = (Job.DiscountAmount / Job.SubTotal) * 100;
            return;
        }

        var materials = Materials.Where(m => !m.IsDeleted);

        if (!materials.Any()) return;

        if (Job.DiscountType == DiscountType.Percentage)
        {
            Job.DiscountAmount = Job.SubTotal * (Job.DiscountPercent / 100);
        }
        else if (Job.DiscountType == DiscountType.Amount)
        {
            Job.DiscountPercent = (Job.DiscountAmount / Job.SubTotal) * 100;
        }

        if (isCalulateMaterials)
        {
            foreach (var material in materials)
            {
                CalculateTotal(material);
            }
        }
        if (Job.DiscountPercent > 0) await DiscountPercentageChanged.InvokeAsync(Job.DiscountPercent);
    }

    private async Task NotifyChanges()
    {
        await MaterialsChanged.InvokeAsync(Materials);
        StateHasChanged();
    }
}
