using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class PurchaseInvoiceStatusEnumDropDown : VanigamSimpleDropDown<PurchaseInvoiceStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_PurchaseInvoiceStatus";
        Data = ((PurchaseInvoiceStatus[])Enum.GetValues(typeof(PurchaseInvoiceStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}