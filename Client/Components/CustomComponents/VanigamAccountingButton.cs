using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents;

public class VanigamAccountingButton : RadzenButton
{
    [CascadingParameter(Name = nameof(TooltipService))]
    public TooltipService TooltipService { get; set; }
    [Parameter]
    public string Tooltip { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        ButtonStyle = ButtonStyle.Primary;
        //ButtonType = ButtonType.Submit;
        Variant = Variant.Flat;
        Size = ButtonSize.Small;
        MouseEnter = EventCallback.Factory.Create<ElementReference>(this, OnMouseEntered); ;
        await base.SetParametersAsync(parameters);
    }

    private void OnMouseEntered(ElementReference elementReference)
    {
        if (!string.IsNullOrEmpty(Tooltip))
            TooltipService.Open(elementReference, Tooltip, new TooltipOptions() { Position = TooltipPosition.Top });
    }
}
