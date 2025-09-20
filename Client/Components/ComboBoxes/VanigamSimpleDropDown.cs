using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class VanigamSimpleDropDown<T> : RadzenDropDown<T>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        AllowClear = false;
        AllowFiltering = true;
        FilterCaseSensitivity = FilterCaseSensitivity.CaseInsensitive;
        Style = "display: block; width: 100%";
        //Data = ((PatientStatus[])Enum.GetValues(typeof(PatientStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}