using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages
{
    public partial class Login
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

        protected string redirectUrl;
        protected string error;
        protected string info;
        protected bool errorVisible;
        protected bool infoVisible;
        protected bool sessionInvalid;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var query = System.Web.HttpUtility.ParseQueryString(new Uri(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString()).Query);

            error = query.Get("error");

            info = query.Get("info");
            bool.TryParse(query.Get("sessionInvalid"),out sessionInvalid);
            if (sessionInvalid)
                error = "You have been logged out because your account was accessed from another device. If this was not you, please secure your account by changing your password.";
            redirectUrl = query.Get("redirectUrl");

            errorVisible = !string.IsNullOrEmpty(error);

            infoVisible = !string.IsNullOrEmpty(info);
        }

        //protected async Task Register()
        //{
        //    var result = await DialogService.OpenDialogAsync<RegisterApplicationUser>("Register Application User");

        //    if (result == true)
        //    {
        //        infoVisible = true;

        //        info = "Registration accepted. Please check your email for further instructions.";
        //    }
        //}

        protected async Task ResetPassword()
        {
            var result = await DialogService.OpenDialogAsync<ResetPassword>("Reset password");

            if (result == true)
            {
                infoVisible = true;

                info = "Password reset successfully. Please check your email for further instructions.";
            }
        }
        string theForm = "document.forms[0]";
        protected async System.Threading.Tasks.Task LoginAsClick(Radzen.Blazor.RadzenSplitButtonItem args)
        {
            if (args?.Text == "Admin")
            {
                await SetLoginCredentials("admin", "admin");
            }
            else if (args?.Text == "Patient")
            {
                await SetLoginCredentials("saravanan@onehs.com", "SarCha@19820416");
            }
            await JSRuntime.InvokeVoidAsync("eval", $@"{theForm}.submit()");
        }

        protected async System.Threading.Tasks.Task SetLoginCredentials(string username, string password)
        {
            await JSRuntime.InvokeVoidAsync("eval", $@"{theForm}.Username.value = '{username}'");
            await JSRuntime.InvokeVoidAsync("eval", $@"{theForm}.Password.value = '{password}'");
        }
    }
}
