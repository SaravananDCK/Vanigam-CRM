using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class QuoteStatusEnumDropDown : VanigamSimpleDropDown<QuoteStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_QuoteStatus";
        Data = ((QuoteStatus[])Enum.GetValues(typeof(QuoteStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}