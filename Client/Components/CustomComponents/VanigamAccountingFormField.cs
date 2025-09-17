using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingFormField : RadzenFormField
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            Variant = Radzen.Variant.Outlined;
            Style = "width: 100%";
            AllowFloatingLabel = false;
            await base.SetParametersAsync(parameters);
        }
    }
}

