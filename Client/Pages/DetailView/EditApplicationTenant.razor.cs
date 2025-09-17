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
    public partial class EditApplicationTenant
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

        protected IEnumerable<Objects.ApplicationRole> roles;
        protected Objects.ApplicationTenant CurrentObject;
        protected string Error;
        protected bool ErrorVisible;
        protected bool IsBusy { get; set; }

        IEnumerable<ApplicationUser> users;
        IList<string> selectedValues;
        private int count;
        [Parameter]
        public int Id { get; set; }

        [Inject]
        protected SecurityService Security { get; set; }
        protected RadzenTemplateForm<ApplicationTenant> Form { get; set; }
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
            if (Id == 0)
                CurrentObject = new();
            else
                CurrentObject = await Security.GetTenantById(Id);
            await InitEditContext();
        }

        protected async Task FormSubmit(Objects.ApplicationTenant tenant)
        {
            IsBusy = true;
            try
            {
                if (Id == 0)
                {
                    await Security.CreateTenant(tenant);
                }
                else
                {

                    await Security.UpdateTenant(Id, tenant);
                }
                    
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
        async Task LoadData(LoadDataArgs args)
        {
            var query = await Security.GetTenantUsers(Id);

            if (!string.IsNullOrEmpty(args.Filter))
            {
                //query = query.Where(c => c.CustomerID.ToLower().Contains(args.Filter.ToLower()) || c.ContactName.ToLower().Contains(args.Filter.ToLower()));
            }

            count = query.Count();

            users = query.Skip(args.Skip.HasValue ? args.Skip.Value : 0).Take(args.Top.HasValue ? args.Top.Value : 10).ToList();

            InvokeAsync(StateHasChanged);
        }
    }
}
