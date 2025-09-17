using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages
{
    public partial class ApplicationUsers
    {
        [Inject] protected DialogService DialogService { get; set; }
        
        protected string error;
        protected bool errorVisible;
        protected bool ShowEdit { get; set; } = false;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            DataSource = await Security.GetUsers();
        }
                
        protected async Task RowSelect(DataGridRowMouseEventArgs<ApplicationUser> user)
        {
            await Open(user.Data);
        }

        private async Task Open(ApplicationUser user)
        {
            ShowEdit = true;
            await ClickReset(user);
        }

        private async Task ClickReset(ApplicationUser user)
        {
            await DialogService.OpenDialogAsync<EditApplicationUser>(Localizer[(ShowEdit == true)?"EditApplicationUser": "Reset Password"], new Dictionary<string, object> { { "Id", user.Id }, { "ShowResetPassword", ShowEdit } });

            DataSource = await Security.GetUsers();

            ShowEdit = false;
        }
    }
}
