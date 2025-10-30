using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class PurchaseOrderStatusEnumDropDown : VanigamSimpleDropDown<PurchaseOrderStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_PurchaseOrderStatus";
        Data = ((PurchaseOrderStatus[])Enum.GetValues(typeof(PurchaseOrderStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}