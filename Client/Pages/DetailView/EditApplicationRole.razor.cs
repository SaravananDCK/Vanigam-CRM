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
using Vanigam.CRM.Objects;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditApplicationRole
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

        protected ApplicationRole CurrentObject;
        protected string Error;
        protected bool ErrorVisible;
        protected bool IsBusy { get; set; }
        protected RadzenTemplateForm<ApplicationRole> Form { get; set; }
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

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            CurrentObject = new();
            await InitEditContext();
        }

        protected async Task FormSubmit(ApplicationRole role)
        {
            IsBusy = true;
            try
            {
                await Security.CreateRole(role);

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
