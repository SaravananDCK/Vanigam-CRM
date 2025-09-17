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
using Vanigam.CRM.Objects;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class ChooseTenant
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

        protected string oldPassword = "";
        protected string newPassword = "";
        protected string confirmPassword = "";
        protected ApplicationUser User;
        protected string error;
        protected bool errorVisible;
        
        protected bool IsBusy { get; set; }

        [Inject]
        protected SecurityService Security { get; set; }
        protected RadzenTemplateForm<ApplicationUser> Form { get; set; }
        protected EditContext EditContext { get; set; }
        private IEnumerable<ApplicationTenant> Tenants { get; set; }
        private int SelectedTenantId { get; set; }
        protected virtual async Task InitEditContext()
        {
            EditContext = new EditContext(User);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }
        protected void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            User = await Security.GetUserById($"{Security.User.Id}");
            Tenants =await Security.GetTenants();
            await InitEditContext();
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                User.TenantId = SelectedTenantId;
                await Security.UpdateUserTenant(User.Id,SelectedTenantId);
                DialogService.CloseDialog();
                await Security.SwitchTenantAndReAuthenticate(User.Id, SelectedTenantId);
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            catch (Exception ex)
            {
                errorVisible = true;
                error = ex.Message;
            }
            IsBusy = false;
        }
        
    }
}
