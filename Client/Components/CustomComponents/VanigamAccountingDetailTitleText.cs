using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingDetailTitleText : RadzenText
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            TextStyle = TextStyle.H5;
            TagName = TagName.H5;
            Style = "color: var(--rz-primary); font-weight: bolder; font-style: italic;";
            await base.SetParametersAsync(parameters);
        }
    }
}

