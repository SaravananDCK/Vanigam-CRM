using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Vanigam.CRM.Client.Pages
{
    public partial class CleanLocalStorage
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("localStorage.clear");
                }
                finally
                {
                    NavigationManager.NavigateTo("/", true);
                }
            }
        }
    }
}