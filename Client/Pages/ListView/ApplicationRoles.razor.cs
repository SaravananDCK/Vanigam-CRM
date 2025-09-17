using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class ApplicationRoles
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

        protected IEnumerable<Objects.ApplicationRole> DataSource;
        protected RadzenDataGrid<Objects.ApplicationRole> GridControl;
        protected string error;
        protected bool errorVisible;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            DataSource = await Security.GetRoles();
        }

        protected async Task AddClick()
        {
            await DialogService.OpenDialogAsync<EditApplicationRole>(Localizer["AddApplicationRole"]);

            DataSource = await Security.GetRoles();
        }
       
    }
}
