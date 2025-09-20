using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class AssetStatusEnumDropDown : VanigamSimpleDropDown<AssetStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_AssetStatus";
        Data = ((AssetStatus[])Enum.GetValues(typeof(AssetStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}