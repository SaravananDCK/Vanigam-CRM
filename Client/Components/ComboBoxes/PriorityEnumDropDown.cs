using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class PriorityEnumDropDown : VanigamSimpleDropDown<Priority>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_Priority";
        Data = ((Priority[])Enum.GetValues(typeof(Priority))).ToList();
        await base.SetParametersAsync(parameters);
    }
}