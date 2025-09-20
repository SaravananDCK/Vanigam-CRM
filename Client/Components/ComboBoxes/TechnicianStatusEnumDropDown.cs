using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class TechnicianStatusEnumDropDown : VanigamSimpleDropDown<TechnicianStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_TechnicianStatus";
        Data = ((TechnicianStatus[])Enum.GetValues(typeof(TechnicianStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}