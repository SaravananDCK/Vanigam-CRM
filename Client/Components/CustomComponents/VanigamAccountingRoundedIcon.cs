using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingRoundedIcon : RadzenIcon
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "rounded-icon-background");

            // Render the base RadzenIcon component
            base.BuildRenderTree(builder);

            // Render the child content
            builder.AddContent(2, ChildContent);

            builder.CloseElement();
        }
    }
}

