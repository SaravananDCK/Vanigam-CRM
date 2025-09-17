using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingAddressValidateButton : RadzenButton
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            ButtonStyle = ButtonStyle.Primary;
            Icon = "check";
            Variant = Variant.Flat;
            await base.SetParametersAsync(parameters);
        }
    }
}

