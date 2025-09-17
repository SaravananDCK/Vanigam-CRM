using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingSaveButton : RadzenButton
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            ButtonStyle = ButtonStyle.Primary;
            ButtonType = ButtonType.Submit;
            Icon = "save";
            Variant = Variant.Flat;
            await base.SetParametersAsync(parameters);
        }
    }
}

