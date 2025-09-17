using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;


namespace Vanigam.CRM.Client.Layout
{
    public partial class LoginLayout
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected SecurityService Security { get; set; }

        private string SubDomain { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SubDomain = await JSRuntime.InvokeAsync<string>("getSubdomain");
                StateHasChanged(); // Ensure the component is re-rendered after retrieving the subdomain
            }
        }

        private string GetLoginURL()
        {
            if (SubDomain?.ToUpper() == "CAPROCK")
            {
                return "images/Caprock Logo.svg";
            }
            else
            {
                return "images/Logo.svg";
            }
            
        }
    }
}
