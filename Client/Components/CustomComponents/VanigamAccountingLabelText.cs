using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingLabelText : RadzenLabel
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            Style = "color: var(--rz-primary);font-weight: bolder; font-style: italic; ";
            await base.SetParametersAsync(parameters);
        }
    }
}

