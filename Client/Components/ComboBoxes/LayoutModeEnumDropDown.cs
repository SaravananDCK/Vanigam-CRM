using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class LayoutModeEnumDropDown : VanigamAccountingSimpleDropDown<LayoutMode>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_LayoutMode";
        Data = ((LayoutMode[])Enum.GetValues(typeof(LayoutMode))).ToList();
        await base.SetParametersAsync(parameters);
    }
}
