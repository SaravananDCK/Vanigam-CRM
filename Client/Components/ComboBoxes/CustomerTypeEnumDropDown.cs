using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class CustomerTypeEnumDropDown : VanigamSimpleDropDown<CustomerType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_CustomerType";
        Data = ((CustomerType[])Enum.GetValues(typeof(CustomerType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}