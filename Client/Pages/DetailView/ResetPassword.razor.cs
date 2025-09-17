using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Components.Forms;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class ResetPassword
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

        protected ApplicationUser User;
        protected bool IsBusy;
        protected bool errorVisible;
        protected string error;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            User = new ApplicationUser();
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                await Security.ResetPassword(User.Email);
                DialogService.CloseDialog(true);
            }
            catch (Exception ex)
            {
                errorVisible = true;
                error = ex.Message;
            }
            IsBusy = false;
        }

        protected async Task CancelClick()
        {
            DialogService.CloseDialog(false);
        }
    }
}
