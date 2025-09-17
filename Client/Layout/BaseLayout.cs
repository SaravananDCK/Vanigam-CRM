using System.Reflection;
using System.Timers;
using Vanigam.CRM.Objects.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Radzen;

namespace Vanigam.CRM.Client.Layout
{
    public class BaseLayout: LayoutComponentBase,IDisposable
    {
        protected string BuildInfo { get; set; }
        protected string ComponentsInfo { get; set; }
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
        
        protected VanigamAccountingOptions VanigamAccountingOptions { get; set; }

        private System.Timers.Timer timerObj;
        private TimeSpan LogoutTimeSpan { get; set; }=new TimeSpan(0,0,15,0);
        private DotNetObjectReference<BaseLayout> DotNetRef { get; set; }
        protected override async Task OnInitializedAsync()
        {
            VanigamAccountingOptions  = await Security.GetVanigamAccountingOptions();
            if (VanigamAccountingOptions?.AutoLogoutMinutes!=null)
            {
                LogoutTimeSpan= new TimeSpan(0, 0, VanigamAccountingOptions.AutoLogoutMinutes.Value, 0);
            }
            Assembly curAssembly = typeof(Program).Assembly;
            BuildInfo = $"{curAssembly.GetCustomAttributes(false).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title}";
            ComponentsInfo = $"{typeof(RadzenComponent).Assembly.GetName().Version.ToString()}";
            // Set the Timer delay.
            timerObj = new System.Timers.Timer(LogoutTimeSpan);
            timerObj.Elapsed += UpdateTimer;
            timerObj.AutoReset = false;
            DotNetRef = DotNetObjectReference.Create(this);
            // Identify whether the user is active or inactive using onmousemove and onkeypress in JS function.
            await JSRuntime.InvokeVoidAsync("timeOutCall", DotNetRef);
            await base.OnInitializedAsync();
        }
        [JSInvokable]
        public void TimerInterval()
        {
            // Resetting the Timer if the user in active state.
            timerObj?.Stop();
            // Call the TimeInterval to logout when the user is inactive.
            timerObj?.Start();
        }

        private void UpdateTimer(Object source, ElapsedEventArgs e)
        {
            InvokeAsync(async () =>
            {
                // Log out when the user is inactive.
                var authState = await Security.GetAuthenticationStateAsync();
                if (authState.IsAuthenticated)
                {
                    NavigationManager.NavigateTo("Account/Logout", true);
                }
            });
        }

        public void Dispose()
        {
            DotNetRef?.Dispose();
            timerObj?.Dispose();
            timerObj = null;
            DialogService?.Dispose();
            TooltipService?.Dispose();
            ContextMenuService?.Dispose();
        }
    }
}

