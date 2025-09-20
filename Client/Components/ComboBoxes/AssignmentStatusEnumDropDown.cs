using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class AssignmentStatusEnumDropDown : VanigamSimpleDropDown<AssignmentStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_AssignmentStatus";
        Data = ((AssignmentStatus[])Enum.GetValues(typeof(AssignmentStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}