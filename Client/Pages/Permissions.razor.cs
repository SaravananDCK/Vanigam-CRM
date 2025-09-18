using ApexCharts;
using DevExpress.Blazor.RichEdit;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Data.Design;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.X509;
using FluentValidation;
using Vanigam.CRM.Client.Components;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Client.Services;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Models;
using Vanigam.CRM.Objects.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Security.Claims;

namespace Vanigam.CRM.Client.Pages
{
    public partial class Permissions
    {
        protected IEnumerable<AuthorizationPermission> DataSource;
        public  RadzenDataGrid<AuthorizationPermission> GridControl { get; set; }
        [Inject] protected NotificationService NotificationService { get; set; }
        [Inject] protected SecurityService Security { get; set; }
        [Inject] protected PermissionApiService PermissionApiService { get; set; }
        protected IEnumerable<Objects.ApplicationRole> Roles;
        protected string UserRole;
        DataGridEditMode editMode = DataGridEditMode.Multiple;
        
        async Task Reset(AuthorizationPermission permission)
        {
           await GridControl.EditRow(permission);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Roles = await Security.GetRoles();
        }
        
        private async Task RolesChange()
        {
            try
            {
                if (UserRole != null)
                {
                    var role = Roles.FirstOrDefault(role => UserRole.Contains(role.Id.ToString()));
                    DataSource = await PermissionApiService.GetClaims(role.Id.ToString());
                    await GridControl.EditRows(DataSource);
                }
                await GridControl.Reload();
            }
            catch(Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Unable to Load Permissions"] });
            }
        }

        async Task CancelChange(AuthorizationPermission permission, string action)
        {
            await GridControl.UpdateRow(permission);
        }

        async Task OnUpdateRow(AuthorizationPermission permission)
        {
            try
            {
                await Reset(permission);
                await PermissionApiService.UpdatePermissions(permission);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Can not Update Permissions"] });
            }
        }
        
    }
}

