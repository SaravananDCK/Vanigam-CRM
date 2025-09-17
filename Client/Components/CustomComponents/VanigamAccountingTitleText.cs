using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingTitleText : RadzenText
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            TextStyle = TextStyle.H4;
            TagName = TagName.H4;
            Style = "margin-top: 0.5rem;color:var(--rz-primary)";
            await base.SetParametersAsync(parameters);
        }
    }
}

