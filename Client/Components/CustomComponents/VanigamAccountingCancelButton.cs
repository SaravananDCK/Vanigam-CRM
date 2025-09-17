using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen;
using Microsoft.AspNetCore.Components.Web;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingCancelButton : RadzenButton
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            ButtonStyle = ButtonStyle.Light;
            Icon = "close";
            Variant = Variant.Flat;
            await base.SetParametersAsync(parameters);
        }
    }
}

