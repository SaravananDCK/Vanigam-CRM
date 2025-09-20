using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents;

public class VanigamFloatingButton : RadzenButton
{
    [CascadingParameter(Name = nameof(TooltipService))]
    public TooltipService TooltipService { get; set; }
    [Parameter]
    public string Tooltip { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Icon = "edit";
        ButtonStyle = ButtonStyle.Primary;
        //ButtonType = ButtonType.Submit;
        Variant = Variant.Flat;
        Size = ButtonSize.Medium;
        MouseEnter = EventCallback.Factory.Create<ElementReference>(this, OnMouseEntered); ;
        Style = "position: fixed; top: 55px; left: 30px; z-index: 1000; border-radius: 50%; width: 60px; height: 60px; box-shadow: 0 4px 8px rgba(0,0,0,0.3);";
        await base.SetParametersAsync(parameters);
    }

    private void OnMouseEntered(ElementReference elementReference)
    {
        if (!string.IsNullOrEmpty(Tooltip))
            TooltipService.Open(elementReference, Tooltip, new TooltipOptions() { Position = TooltipPosition.Top });
    }
}