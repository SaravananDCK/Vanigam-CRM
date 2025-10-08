using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ActivityStatusEnumDropDown : VanigamSimpleDropDown<ActivityStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_ActivityStatus";
        Data = ((ActivityStatus[])Enum.GetValues(typeof(ActivityStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}