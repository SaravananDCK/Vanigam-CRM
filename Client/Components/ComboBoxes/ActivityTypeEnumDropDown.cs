using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ActivityTypeEnumDropDown : VanigamSimpleDropDown<ActivityType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_ActivityType";
        Data = ((ActivityType[])Enum.GetValues(typeof(ActivityType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}