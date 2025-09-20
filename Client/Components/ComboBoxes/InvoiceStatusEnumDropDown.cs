using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class InvoiceStatusEnumDropDown : VanigamSimpleDropDown<InvoiceStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_InvoiceStatus";
        Data = ((InvoiceStatus[])Enum.GetValues(typeof(InvoiceStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}