using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingTextBox : RadzenTextBox
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            Style = "display: block; width: 100%";
            await base.SetParametersAsync(parameters);
        }
    }
}

