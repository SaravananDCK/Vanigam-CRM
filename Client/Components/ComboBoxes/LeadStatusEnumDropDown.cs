using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class LeadStatusEnumDropDown : VanigamSimpleDropDown<LeadStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_LeadStatus";
        Data = ((LeadStatus[])Enum.GetValues(typeof(LeadStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}