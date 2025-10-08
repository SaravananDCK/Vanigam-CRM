using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ContractDurationEnumDropDown : VanigamSimpleDropDown<ContractDuration>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "ddl_ContractDuration";
        Data = ((ContractDuration[])Enum.GetValues(typeof(ContractDuration))).ToList();
        await base.SetParametersAsync(parameters);
    }
}
