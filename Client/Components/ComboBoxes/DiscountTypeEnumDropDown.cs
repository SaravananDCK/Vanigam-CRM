using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class DiscountTypeEnumDropDown : VanigamSimpleDropDown<DiscountType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_DiscountType";
        Data = ((DiscountType[])Enum.GetValues(typeof(DiscountType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}