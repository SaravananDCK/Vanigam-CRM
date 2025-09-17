using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingReadOnlylText : RadzenText
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            TextStyle = TextStyle.H5;
            Style = "font-weight: bolder; margin-top: 0.5rem";
            await base.SetParametersAsync(parameters);
        }
    }
}

