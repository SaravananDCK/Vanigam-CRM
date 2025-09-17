using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class ApplicationTenants
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

        protected string error;
        protected bool errorVisible;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await Load();
        }

        protected async Task Load()
        {
            DataSource = await Security.GetTenants();

            if (Security.Tenant == null && DataSource != null && DataSource.Any())
            {
                Security.Tenant = DataSource.FirstOrDefault();
            }
        }

        protected async Task AddClick()
        {
            await DialogService.OpenDialogAsync<EditApplicationTenant>(Localizer["AddApplicationTenant"]);
            await Load();
        }

        protected async Task SetDefaultTenant(MouseEventArgs args, Objects.ApplicationTenant tenant)
        {
            Security.Tenant = tenant;
        }

        protected async Task RowSelect(DataGridRowMouseEventArgs<Objects.ApplicationTenant> args)
        {
            await DialogService.OpenDialogAsync<EditApplicationTenant>(Localizer["EditApplicationTenant"], new Dictionary<string, object> { { "Id", args.Data.Id } });
            await Load();
        }

        protected async void RowRender(RowRenderEventArgs<Objects.ApplicationTenant> args)
        {
            args.Attributes.Add("style", $"font-weight: {(args.Data.Id == Security.Tenant?.Id ? "bold" : "normal")};");
        }

        protected async Task DeleteClick(Objects.ApplicationTenant tenant)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    await Security.DeleteTenant(tenant.Id);

                    await Load();
                }
            }
            catch (Exception ex)
            {
                errorVisible = true;
                error = ex.Message;
            }
        }
    }
}
