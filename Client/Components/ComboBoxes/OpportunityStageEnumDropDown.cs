using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class OpportunityStageEnumDropDown : VanigamSimpleDropDown<OpportunityStage>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_OpportunityStage";
        Data = ((OpportunityStage[])Enum.GetValues(typeof(OpportunityStage))).ToList();
        await base.SetParametersAsync(parameters);
    }
}