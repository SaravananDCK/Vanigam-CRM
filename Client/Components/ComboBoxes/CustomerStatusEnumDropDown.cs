using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class CustomerStatusEnumDropDown : VanigamSimpleDropDown<CustomerStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_CustomerStatus";
        Data = ((CustomerStatus[])Enum.GetValues(typeof(CustomerStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}