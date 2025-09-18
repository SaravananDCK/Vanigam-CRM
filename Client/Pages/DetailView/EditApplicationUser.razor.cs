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
    public partial class EditApplicationUser
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

        protected IEnumerable<Objects.ApplicationRole> Roles;
        protected ApplicationUser CurrentObject;
        protected IEnumerable<string> UserRoles;
        protected string Error;
        protected bool ErrorVisible;
        protected bool IsBusy { get; set; }

        [Parameter]
        public string Id { get; set; }
        [Parameter]
        public bool ShowResetPassword { get; set; } = true;

        [Inject]
        protected SecurityService Security { get; set; }
        protected RadzenTemplateForm<ApplicationUser> Form { get; set; }
        protected EditContext EditContext { get; set; }
        protected virtual async Task InitEditContext()
        {
            EditContext = new EditContext(CurrentObject);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }
        protected void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (string.IsNullOrEmpty(Id))
            {
                CurrentObject = new();
            }
            else
            {
                CurrentObject = await Security.GetUserById($"{Id}");
                UserRoles = CurrentObject.Roles.Select(role => role.Id.ToString());
            }

            Roles = await Security.GetRoles();
            await InitEditContext();
        }

        protected async Task FormSubmit(ApplicationUser user)
        {
            IsBusy = true;
            try
            {
                user.Roles = Roles.Where(role => UserRoles.Contains(role.Id.ToString())).ToList();
                if (string.IsNullOrEmpty(Id))
                    await Security.CreateUser(user);
                else
                    await Security.UpdateUser($"{Id}", user);
                DialogService.CloseDialog(null);
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
                Error = ex.Message;
            }
            IsBusy = false;
        }

        protected async Task CancelClick()
        {
            DialogService.CloseDialog(null);
        }
    }
}
