using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class vanigamAccountingPhoneMask: RadzenMask
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            Mask = "(***) ***-****";
            CharacterPattern = "[0-9]"; 
            Style = "display: block; width: 100%";
            await base.SetParametersAsync(parameters);
        }
    }
}

