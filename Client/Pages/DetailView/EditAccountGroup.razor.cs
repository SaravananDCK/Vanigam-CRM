using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Services;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditAccountGroup
    {
        [Inject] private AccountGroupApiService AccountGroupApiService { get; set; }

        private IEnumerable<AccountGroup> ParentGroups { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new() { IsActive = true };
            else
                CurrentObject = await AccountGroupApiService.GetByOid(oid: Oid);

            await LoadParentGroups();
            await InitEditContext();
        }

        private async Task LoadParentGroups()
        {
            try
            {
                var filter = Oid != Guid.Empty ? $"Oid ne {Oid}" : null; // Prevent circular reference
                var result = await AccountGroupApiService.Get(filter: filter, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                ParentGroups = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadParentGroupsFailed"] });
            }
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await AccountGroupApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await AccountGroupApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = Localizer["SavedSuccessfully!"] });
                DialogService.CloseDialog(CurrentObject);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    ShowNotUniqueAlert = true;
                }
                else
                {
                    ErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
            }
            IsBusy = false;
        }
    }
}
