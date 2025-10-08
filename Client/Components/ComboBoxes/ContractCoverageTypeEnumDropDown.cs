using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ContractCoverageTypeEnumDropDown : VanigamSimpleDropDown<ContractCoverageType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_ContractCoverageType";
        Data = ((ContractCoverageType[])Enum.GetValues(typeof(ContractCoverageType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}