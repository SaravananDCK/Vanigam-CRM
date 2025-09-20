using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class AppointmentStatusEnumDropDown : VanigamSimpleDropDown<AppointmentStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_AppointmentStatus";
        Data = ((AppointmentStatus[])Enum.GetValues(typeof(AppointmentStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}